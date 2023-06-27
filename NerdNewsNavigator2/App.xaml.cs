// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2;

/// <summary>
/// A class that acts as a manager for <see cref="Application"/>
/// </summary>
public partial class App : Application
{
    public static bool Stop { get; set; } = false;
    /// <summary>
    /// This applications Dependancy Injection for <see cref="PositionDataBase"/> class.
    /// </summary>
    public static PositionDataBase PositionData { get; private set; }

    private readonly IMessenger _messenger;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="positionDataBase"></param>
    public App(PositionDataBase positionDataBase, IMessenger messenger)
    {
        InitializeComponent();
#if ANDROID
        // Local Notification tap event listener
        LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;
#endif
        MainPage = new AppShell();

        _messenger = messenger;
        // Database Dependancy Injection START
        PositionData = positionDataBase;
        // Database Dependancy Injection END
#if ANDROID
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
                        new NotificationAction(101)
                        {
                            Title ="Close Notification",
                        },
                    })
                }
            }));
#endif
        LogController.InitializeNavigation(
            page => MainPage!.Navigation.PushModalAsync(page),
            () => MainPage!.Navigation.PopModalAsync());
        ThreadPool.QueueUserWorkItem(state =>
        {
            StartAutoDownloadService();
        });
    }

#if ANDROID

    private void OnNotificationActionTapped(Plugin.LocalNotification.EventArgs.NotificationActionEventArgs e)
    {
        switch (e.ActionId)
        {
            case 100:
                DownloadService.CancelDownload = true;
                break;
            case 101:
                Stop = true;
                LocalNotificationCenter.Current.Cancel(e.Request.NotificationId);
                break;
            default:
                Shell.Current.GoToAsync($"{nameof(DownloadedShowPage)}");
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
}

