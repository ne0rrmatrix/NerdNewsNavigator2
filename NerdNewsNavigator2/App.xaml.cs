// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2;

/// <summary>
/// A class that acts as a manager for <see cref="Application"/>
/// </summary>
public partial class App : Application
{
    #region Properties
    public MessagingService MessagingService { get; set; } = new();
    public static AutoDownloadService AutoDownloadService { get; set; }

    /// <summary>
    /// This applications Dependency Injection for <see cref="PositionDataBase"/> class.
    /// </summary>
    public static PositionDataBase PositionData { get; private set; }

    private readonly IMessenger _messenger;
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(App));
    private readonly IDownloadService _downloadService;
    private readonly ICurrentDownloads _currentDownloads;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="positionDataBase"></param>
    public App(PositionDataBase positionDataBase, IMessenger messenger, IDownloadService downloadService, ICurrentDownloads currentDownloads)
    {
        InitializeComponent();

        MainPage = new AppShell();
        _messenger = messenger;
        _downloadService = downloadService;
        _currentDownloads = currentDownloads;
        // Database Dependency Injection START
        PositionData = positionDataBase;
        _currentDownloads.DownloadFinished += DownloadDone;
        _currentDownloads.DownloadCancelled += DownloadDone;
        // Database Dependency Injection END
        LogController.InitializeNavigation(
           page => MainPage!.Navigation.PushModalAsync(page),
           () => MainPage!.Navigation.PopModalAsync());
#if ANDROID || IOS
        // Local Notification tap event listener
        LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;
        LocalNotificationCenter.Current.RegisterCategoryList(new HashSet<NotificationCategory>(new List<NotificationCategory>()
            {
                new(NotificationCategoryType.Status)
                {
                    ActionList = new HashSet<NotificationAction>( new List<NotificationAction>()
                    {
                        new(103)
                        {
                            Title = "Close Notification",
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

    private void DownloadDone(object sender, DownloadEventArgs e)
    {
        if (e.Shows.Count > 0)
        {
            _logger.Info($"Starting next show: {e.Shows[0].Title}");
            _ = _downloadService.Start(e.Shows[0]);
        }
    }
    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Destroying += (s, e) =>
        {
            _downloadService.CancelAll();
            Thread.Sleep(50);
            _logger.Info("Safe shutdown completed");
        };
        return window;
    }

#if ANDROID || IOS

    private void OnNotificationActionTapped(Plugin.LocalNotification.EventArgs.NotificationActionEventArgs e)
    {
        switch (e.ActionId)
        {
            case 103:
                e.Request.Cancel();
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
