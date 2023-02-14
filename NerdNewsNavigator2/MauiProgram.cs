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

        builder.Services.AddSingleton<BaseViewModel>();

        builder.Services.AddTransient<TabletPodcastPage>();
        builder.Services.AddTransient<TabletPodcastViewModel>();

        builder.Services.AddTransient<TabletShowPage>();
        builder.Services.AddTransient<TabletShowViewModel>();

        builder.Services.AddSingleton<TabletPlayPodcastPage>();
        builder.Services.AddSingleton<TabletPlayPodcastViewModel>();

        builder.Services.AddSingleton<LivePage>();
        builder.Services.AddSingleton<LiveViewModel>();

        builder.Services.AddSingleton<AddPodcastPage>();
        builder.Services.AddSingleton<AddPodcastViewModel>();

        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<SettingsViewModel>();

        builder.Services.AddTransient<UpdateSettingsPage>();
        builder.Services.AddTransient<UpdateSettingsViewModel>();

        builder.Services.AddSingleton<FeedService>();
        //   builder.Services.AddSingleton<PositionServices>();
        //   builder.Services.AddSingleton<PlaybackService>();
        builder.Services.AddSingleton<PodcastServices>();

        builder.Services.AddSingleton<PositionDataBase>();

        return builder.Build();
    }
}
