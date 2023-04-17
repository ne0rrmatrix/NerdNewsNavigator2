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
    private bool IsRunning { get; set; } = true;
    private IConnectivity _connectivity;
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
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
        _connectivity = MauiWinUIApplication.Current.Services.GetService<IConnectivity>();
        var messenger = MauiWinUIApplication.Current.Services.GetService<IMessenger>();
        messenger.Register<MessageData>(this, (recipient, message) =>
        {
            if (message.Start && InternetConnected())
            {
                Start();
            }
            else
            {
                Stop();
            }
        });
    }

    /// <summary>
    /// A method that checks if the internet is connected and returns a <see cref="bool"/> as answer.
    /// </summary>
    /// <returns></returns>
    public bool InternetConnected()
    {
        if (_connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// A method that Auto starts Downloads
    /// </summary>
    public void Start()
    {
        while (IsRunning)
        {
            ThreadPool.QueueUserWorkItem(async state =>
            {
                await NerdNewsNavigator2.App.AutoDownload();
            });
            Thread.Sleep(1000 * 60 * 60);
        }
    }
    /// <summary>
    /// A method that Stops auto downloads
    /// </summary>
    public void Stop()
    {
        IsRunning = false;
    }
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

