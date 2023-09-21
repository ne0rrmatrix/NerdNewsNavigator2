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
    AutoDownloadService AutoDownloadService { get; set; }
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
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
        AutoDownloadService = NerdNewsNavigator2.App.AutoDownloadService;
        var messenger = MauiWinUIApplication.Current.Services.GetService<IMessenger>();
        messenger.Register<MessageData>(this, async (recipient, message) =>
        {
            if (message.Start)
            {
                if (AutoDownloadService.CancellationTokenSource is null)
                {
                    var cts = new CancellationTokenSource();
                    AutoDownloadService.CancellationTokenSource = cts;
                }
                else if (AutoDownloadService.CancellationTokenSource is not null)
                {
                    AutoDownloadService.CancellationTokenSource.Dispose();
                    AutoDownloadService.CancellationTokenSource = null;
                    var cts = new CancellationTokenSource();
                    AutoDownloadService.CancellationTokenSource = cts;
                }
                await AutoDownloadService.LongTaskAsync(AutoDownloadService.CancellationTokenSource.Token);
            }
            else
            {
                AutoDownloadService.Stop();
            }
        });
    }
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

