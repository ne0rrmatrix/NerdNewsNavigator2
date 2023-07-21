// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2;

/// <summary>
/// A class that acts as a manager for <see cref="Application"/>
/// </summary>
public partial class App : Application, IRecipient<NotificationItemMessage>
{
    #region Properties
    public static Show ShowItem { get; set; } = new();
    public static VideoOnNavigated OnVideoNavigated { get; set; } = new();
    public static bool Loading { get; set; } = false;
    public static List<Show> MostRecentShows { get; set; } = new();
    public static List<Message> Message { get; set; } = new();
    public static CurrentDownloads Downloads { get; set; } = new();
    public static CurrentNavigation CurrentNavigation { get; set; } = new();
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

        MainPage = new AppShell();
        _messenger = messenger;
        // Database Dependancy Injection START
        PositionData = positionDataBase;
        // Database Dependancy Injection END
        LogController.InitializeNavigation(
           page => MainPage!.Navigation.PushModalAsync(page),
           () => MainPage!.Navigation.PopModalAsync());
        ThreadPool.QueueUserWorkItem(async state => await GetMostRecent());
#if ANDROID || IOS
        // Local Notification tap event listener
        WeakReferenceMessenger.Default.Register<NotificationItemMessage>(this);
        LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;
        LocalNotificationCenter.Current.RegisterCategoryList(new HashSet<NotificationCategory>(new List<NotificationCategory>()
            {
                new NotificationCategory(NotificationCategoryType.Status)
                {
                    ActionList = new HashSet<NotificationAction>( new List<NotificationAction>()
                    {
                        new NotificationAction(103)
                        {
                            Title = "Play",
                        }
                    })
                }
            }));
#endif

        ThreadPool.QueueUserWorkItem(state =>
        {
            StartAutoDownloadService();
        });
    }
    public static async Task GetMostRecent()
    {
        Loading = true;
        MostRecentShows.Clear();
        var temp = await App.PositionData.GetAllPodcasts();
        while (temp.Count == 0)
        {
            Thread.Sleep(5000);
            temp = await App.PositionData.GetAllPodcasts();
        }
        temp?.Where(x => !x.Deleted).ToList().ForEach(show =>
        {
            var item = FeedService.GetShows(show.Url, true);
            if (item.Count > 0)
            {

                MostRecentShows.Add(item[0]);
            }
        });
        Loading = false;
        Debug.WriteLine("Got App most recent shows");
    }
    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Destroying += (s, e) =>
        {
            Thread.Sleep(500);
            Debug.WriteLine("Safe shutdown completed");
        };
        return window;
    }

#if ANDROID || IOS

    private void OnNotificationActionTapped(Plugin.LocalNotification.EventArgs.NotificationActionEventArgs e)
    {
        var message = Message.First(item => item.Id == e.Request.NotificationId);
        switch (e.ActionId)
        {
            case 103:
                var item = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DownloadService.GetFileName(message.Url));
                Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}?Url={item}");
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
        WeakReferenceMessenger.Default.Unregister<NotificationItemMessage>(message);
    }
    #endregion
}

