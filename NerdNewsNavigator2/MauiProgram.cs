// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif
using NerdNewsNavigator2.Data;

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
        builder.Services.AddSingleton<FirstPage>();
        builder.Services.AddSingleton<FirstVieModel>();

        builder.Services.AddSingleton<PhonePodcastPage>();
        builder.Services.AddSingleton<PhonePodcastViewModel>();

        builder.Services.AddSingleton<PhoneShowPage>();
        builder.Services.AddSingleton<PhoneShowViewModel>();

        builder.Services.AddSingleton<PhonePlayPodcastPage>();
        builder.Services.AddSingleton<PhonePlayPodcastViewModel>();

        builder.Services.AddSingleton<PhoneLivePage>();
        builder.Services.AddSingleton<PhoneLiveViewModel>();

        builder.Services.AddSingleton<DesktopPodcastPage>();
        builder.Services.AddSingleton<DesktopPodcastViewModel>();

        builder.Services.AddSingleton<DesktopShowPage>();
        builder.Services.AddSingleton<DesktopShowViewModel>();

        builder.Services.AddSingleton<DesktopPlayPodcastPage>();
        builder.Services.AddSingleton<DesktopPlayPodcastViewModel>();

        builder.Services.AddSingleton<DesktopLivePage>();
        builder.Services.AddSingleton<DesktopLiveViewModel>();

        builder.Services.AddSingleton<TabletPodcastPage>();
        builder.Services.AddSingleton<TabletPodcastViewModel>();

        builder.Services.AddSingleton<TabletShowPage>();
        builder.Services.AddSingleton<TabletShowViewModel>();

        builder.Services.AddSingleton<TabletPlayPodcastPage>();
        builder.Services.AddSingleton<TabletPlayPodcastViewModel>();

        builder.Services.AddSingleton<TabletLivePage>();
        builder.Services.AddSingleton<TabletLiveViewModel>();

        builder.Services.AddSingleton<LivePage>();
        builder.Services.AddSingleton<LiveViewModel>();

        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<SettingsViewModel>();

        builder.Services.AddSingleton<FeedService>();
        builder.Services.AddSingleton<PlaybackService>();
        builder.Services.AddSingleton<PositionServices>();
        builder.Services.AddSingleton<PodcastServices>();
        builder.Services.AddSingleton<PositionDataBase>();

        return builder.Build();
    }
}
