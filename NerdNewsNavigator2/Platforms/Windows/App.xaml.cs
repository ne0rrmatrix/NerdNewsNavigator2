// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.UI.Windowing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NerdNewsNavigator2.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    private readonly System.Timers.Timer _aTimer = new(60 * 60 * 1000);
    private string WifiOnlyDownloading { get; set; }
    private string Status { get; set; }
    public static CancellationTokenSource CancellationTokenSource { get; set; } = null;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
        WifiOnlyDownloading = Preferences.Default.Get("WifiOnly", "No");
        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        {
            var nativeWindow = handler.PlatformView;
            nativeWindow.Activate();
            // allow Windows to draw a native titlebar which respects IsMaximizable/IsMinimizable
            nativeWindow.ExtendsContentIntoTitleBar = false;

            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            // set a specific window size
            if (appWindow.Presenter is OverlappedPresenter p)
            {
                p.IsResizable = true;
                // these only have effect if XAML isn't responsible for drawing the titlebar.
                p.IsMaximizable = true;
                p.IsMinimizable = true;
            }
        });
    }
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        var messenger = MauiWinUIApplication.Current.Services.GetService<IMessenger>();
        messenger.Register<MessageData>(this, (recipient, message) =>
        {
            if (message.Start)
            {
                Start();
                Task.Run(() =>
                {
                    Thread.Sleep(60 * 1000);
                    _ = DownloadService.AutoDownload();
                });
            }
            else
            {
                Stop();
            }
        });
    }

    /// <summary>
    /// A method that Auto starts Downloads
    /// </summary>
    public void Start()
    {
        if (CancellationTokenSource is null)
        {
            var cts = new CancellationTokenSource();
            CancellationTokenSource = cts;
        }
        else if (CancellationTokenSource is not null)
        {
            CancellationTokenSource.Dispose();
            CancellationTokenSource = null;
            var cts = new CancellationTokenSource();
            CancellationTokenSource = cts;
        }
        Debug.WriteLine("Start Auto downloads");
        LongTask(CancellationTokenSource.Token);
    }
    /// <summary>
    /// A method that Stops auto downloads
    /// </summary>
    public void Stop()
    {
        if (CancellationTokenSource is not null)
        {
            CancellationTokenSource.Cancel();
            LongTask(CancellationTokenSource.Token);
            CancellationTokenSource?.Dispose();
            CancellationTokenSource = null;
        }
        Debug.WriteLine("Stopped Auto Downloder");
    }

    public void LongTask(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _aTimer.Stop();
            Connectivity.Current.ConnectivityChanged -= GetCurrentConnectivity;
            _aTimer.Elapsed -= new System.Timers.ElapsedEventHandler(OnTimedEvent);
            return;
        }
        _aTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
        Connectivity.Current.ConnectivityChanged += GetCurrentConnectivity;
        _aTimer.Start();
    }
    private void GetCurrentConnectivity(object sender, ConnectivityChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Connection status has changed");
        Status = string.Join(", ", Connectivity.Current.ConnectionProfiles);
        WifiOnlyDownloading = Preferences.Default.Get("WifiOnly", "No");
        if (Status == string.Empty)
        {
            Debug.WriteLine("No Connection to internet available");
            return;
        }
        System.Diagnostics.Debug.WriteLine(Status);
    }
    private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
    {
        var connectionStatus = Connectivity.Current.ConnectionProfiles;
        connectionStatus.ToList().ForEach(item =>
        {
            switch (item)
            {
                case ConnectionProfile.WiFi:
                case ConnectionProfile.Ethernet:
                    Debug.WriteLine($"Timed event: {e} Started");
                    _ = DownloadService.AutoDownload();
                    break;
                case ConnectionProfile.Cellular:
                    if (WifiOnlyDownloading == "No")
                    {
                        Debug.WriteLine($"Timed event: {e} Started");
                        _ = DownloadService.AutoDownload();
                    }
                    break;
            }
        });
    }
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

