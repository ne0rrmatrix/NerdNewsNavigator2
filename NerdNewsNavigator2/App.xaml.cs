// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AngleSharp.Dom;

namespace NerdNewsNavigator2;

/// <summary>
/// A class that acts as a manager for <see cref="Application"/>
/// </summary>
public partial class App : Application, IRecipient<NotificationItemMessage>, IRecipient<InternetItemMessage>, IRecipient<DownloadItemMessage>, IRecipient<UrlItemMessage>
{
    #region Properties
    public static Show ShowItem { get; set; } = new();
    public static List<Show> AllShows { get; set; } = new();
    public static bool Stop { get; set; } = false;
    public static bool Started { get; set; } = false;
    public static List<Message> Message { get; set; } = new();
    /// <summary>
    /// This applications Dependancy Injection for <see cref="PositionDataBase"/> class.
    /// </summary>
    public static PositionDataBase PositionData { get; private set; }

    private readonly IMessenger _messenger;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="positionDataBase"></param>
    public App(PositionDataBase positionDataBase, IMessenger messenger)
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<DownloadItemMessage>(this);
        WeakReferenceMessenger.Default.Register<InternetItemMessage>(this);
        WeakReferenceMessenger.Default.Register<UrlItemMessage>(this);
        MainPage = new AppShell();
        _messenger = messenger;
        // Database Dependancy Injection START
        PositionData = positionDataBase;
        // Database Dependancy Injection END
        LogController.InitializeNavigation(
           page => MainPage!.Navigation.PushModalAsync(page),
           () => MainPage!.Navigation.PopModalAsync());
        Task.Run(async () =>
        {
            await GetMostRecent();
        });
#if ANDROID || IOS
        // Local Notification tap event listener
        WeakReferenceMessenger.Default.Register<NotificationItemMessage>(this);
        LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;

        LocalNotificationCenter.Current.RegisterCategoryList(new HashSet<NotificationCategory>(new List<NotificationCategory>()
            {
                new NotificationCategory(NotificationCategoryType.Progress)
                {
                    ActionList = new HashSet<NotificationAction>( new List<NotificationAction>()
                    {
                        new NotificationAction(100)
                        {
                            Title = "Stop Download",
                        },
                    })
                }

            }));
        LocalNotificationCenter.Current.RegisterCategoryList(new HashSet<NotificationCategory>(new List<NotificationCategory>()
            {
                new NotificationCategory(NotificationCategoryType.None)
                {
                    ActionList = new HashSet<NotificationAction>( new List<NotificationAction>()
                    {
                        new NotificationAction(101)
                        {
                            Title ="Close Notification",
                        },
                    })
                }
            }));
#endif

        ThreadPool.QueueUserWorkItem(state =>
        {
            StartAutoDownloadService();
        });
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Destroying += (s, e) =>
        {
            DownloadService.CancelDownload = true;
            Thread.Sleep(500);
            Debug.WriteLine("Safe shutdown completed");
        };
        return window;
    }
    /// <summary>
    /// Method gets most recent episode from each podcast on twit.tv
    /// </summary>
    /// <returns></returns>
    public static async Task GetMostRecent()
    {
        Started = true;
        AllShows.Clear();
        var temp = await App.PositionData.GetAllPodcasts();
        var downloads = await App.PositionData.GetAllDownloads();
        var result = new List<Show>();
        temp?.Where(x => !x.Deleted).ToList().ForEach(show =>
        {
            var item = FeedService.GetShows(show.Url, true);
            if (downloads.Exists(y => y.Url == item[0].Url))
            {
                item[0].IsDownloaded = true;
                item[0].IsNotDownloaded = false;
                item[0].IsDownloading = false;
            }
            result.Add(item[0]);
        });
        var item = BaseViewModel.RemoveDuplicates(result);
        item.ForEach(AllShows.Add);
        Started = false;
        Debug.WriteLine("Got Most recent shows");
    }

#if ANDROID || IOS

    private void OnNotificationActionTapped(Plugin.LocalNotification.EventArgs.NotificationActionEventArgs e)
    {
        var message = Message.First(item => item.Id == e.Request.NotificationId);
        switch (e.ActionId)
        {
            case 100:
                if (e.Request.NotificationId == message.Id)
                {
                    DownloadService.CancelDownload = true;
                }
                else
                {
                    DownloadService.CancelDownload = true;
                }
                break;
            case 101:
                Stop = true;
                LocalNotificationCenter.Current.Cancel(message.Id);
                break;
            default:
                if (message.Cancel)
                {
                    LocalNotificationCenter.Current.Cancel(e.Request.NotificationId);
                    break;
                }
                if (e.Request.Cancel())
                {
                    LocalNotificationCenter.Current.Cancel(e.Request.NotificationId);
                    break;
                }
                if (e.Request.NotificationId == message.Id)
                {
                    var item = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DownloadService.GetFileName(message.Url));
                    Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}?Url={item}");
                }
                else
                {
                    Shell.Current.GoToAsync($"{nameof(PodcastPage)}");
                }
                break;
        }
    }

#endif
    private void StartAutoDownloadService()
    {
        Thread.Sleep(5000);
        var start = Preferences.Default.Get("start", false);
        if (start)
        {
            _messenger.Send(new MessageData(true));
        }
    }

    #region Messaging Service

    /// <summary>
    /// Method invokes <see cref="MessagingService.RecievedDownloadMessage(bool,string)"/> for displaying <see cref="Toast"/>
    /// </summary>
    /// <param name="message"></param>
    public void Receive(DownloadItemMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await MessagingService.RecievedDownloadMessage(message.Value, message.Title);
        });
        WeakReferenceMessenger.Default.Unregister<DownloadItemMessage>(message);
    }

    /// <summary>
    /// Method invokes <see cref="MessagingService.RecievedInternetMessage(bool)"/> for displaying <see cref="Toast"/>
    /// </summary>
    /// <param name="message"></param>
    public void Receive(InternetItemMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await MessagingService.RecievedInternetMessage(message.Value);
        });
        WeakReferenceMessenger.Default.Unregister<InternetItemMessage>(message);
    }
    public void Receive(NotificationItemMessage message)
    {
        var newMessage = new Message
        {
            Cancel = message.Cancel,
            Id = message.Id,
            Url = message.Url
        };
        if (Message.Exists(x => x.Id == message.Id))
        {
            var item = Message.First(x => x.Id == message.Id);
            item.Cancel = message.Cancel;
            Message[Message.IndexOf(item)] = item;
        }
        else
        {
            Message.Add(newMessage);
        }
        WeakReferenceMessenger.Default.Reset();
        WeakReferenceMessenger.Default.Register<NotificationItemMessage>(this);
    }
    public void Receive(UrlItemMessage message)
    {
        ShowItem = message.ShowItem;
        WeakReferenceMessenger.Default.Reset();
        WeakReferenceMessenger.Default.Register<UrlItemMessage>(this);
    }
    #endregion
}

