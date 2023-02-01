// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

namespace NerdNewsNavigator2;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        }).UseMauiCommunityToolkit().UseMauiCommunityToolkitMediaElement();

#if WINDOWS
        builder.ConfigureLifecycleEvents(events =>
               {
                   events.AddWindows(wndLifeCycleBuilder =>
            {
                wndLifeCycleBuilder.OnWindowCreated(window =>
                {
                    window.ExtendsContentIntoTitleBar = false;
                    IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
                    var appWindow = AppWindow.GetFromWindowId(myWndId);
                    appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                });
            });
               });
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddTransient<FirstPage>();
        builder.Services.AddTransient<FirstVieModel>();

        builder.Services.AddSingleton<PhonePodcastPage>();
        builder.Services.AddSingleton<PhonePodcastViewModel>();

        builder.Services.AddTransient<PhoneShowPage>();
        builder.Services.AddTransient<PhoneShowViewModel>();

        builder.Services.AddSingleton<PhonePlayPodcastPage>();
        builder.Services.AddSingleton<PhonePlayPodcastViewModel>();

        builder.Services.AddTransient<PhoneLivePage>();
        builder.Services.AddTransient<PhoneLiveViewModel>();

        builder.Services.AddSingleton<DesktopPodcastPage>();
        builder.Services.AddSingleton<DesktopPodcastViewModel>();

        builder.Services.AddTransient<DesktopShowPage>();
        builder.Services.AddTransient<DesktopShowViewModel>();

        builder.Services.AddTransient<DesktopPlayPodcastPage>();
        builder.Services.AddTransient<DesktopPlayPodcastViewModel>();

        builder.Services.AddTransient<DesktopLivePage>();
        builder.Services.AddTransient<DesktopLiveViewModel>();

        builder.Services.AddSingleton<TabletPodcastPage>();
        builder.Services.AddSingleton<TabletPodcastViewModel>();

        builder.Services.AddTransient<TabletShowPage>();
        builder.Services.AddTransient<TabletShowViewModel>();

        builder.Services.AddTransient<TabletPlayPodcastPage>();
        builder.Services.AddTransient<TabletPlayPodcastViewModel>();

        builder.Services.AddTransient<TabletLivePage>();
        builder.Services.AddTransient<TabletLiveViewModel>();

        builder.Services.AddTransient<LivePage>();
        builder.Services.AddTransient<LiveViewModel>();

        builder.Services.AddSingleton<TwitService>();
        builder.Services.AddSingleton<FeedService>();

        return builder.Build();
    }
}
