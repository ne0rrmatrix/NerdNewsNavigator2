// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

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
        builder.Logging

#if DEBUG
           .AddTraceLogger(
                options =>
                {
                    options.MinLevel = LogLevel.Trace;
                    options.MaxLevel = LogLevel.Critical;
                }) // Will write to the Debug Output
#endif
            .AddInMemoryLogger(
                options =>
                {
                    options.MaxLines = 1024;
                    options.MinLevel = LogLevel.Debug;
                    options.MaxLevel = LogLevel.Critical;
                })
#if RELEASE
            .AddStreamingFileLogger(
                options =>
                {
                    options.RetainDays = 2;
                    options.FolderPath = Path.Combine(
                        FileSystem.CacheDirectory,
                        "MetroLogs");
                })
#endif
            .AddConsoleLogger(
                options =>
                {
                    options.MinLevel = LogLevel.Information;
                    options.MaxLevel = LogLevel.Critical;
                }); // Will write to the Console Output (logcat for android)

        builder.Services.AddSingleton<BaseViewModel>();

        builder.Services.AddTransient<TabletPodcastPage>();
        builder.Services.AddTransient<TabletPodcastViewModel>();

        builder.Services.AddTransient<TabletShowPage>();
        builder.Services.AddTransient<TabletShowViewModel>();
#if WINDOWS || ANDROID
        builder.Services.AddSingleton<TabletPlayPodcastPage>();
#endif
#if IOS
        builder.Services.AddTransient<TabletPlayPodcastPage>();
#endif
        builder.Services.AddTransient<TabletPlayPodcastViewModel>();

        builder.Services.AddTransient<LivePage>();
        builder.Services.AddTransient<LiveViewModel>();

        builder.Services.AddTransient<AddPodcastPage>();
        builder.Services.AddTransient<AddPodcastViewModel>();

        builder.Services.AddTransient<RemovePage>();
        builder.Services.AddTransient<RemoveViewModel>();

        builder.Services.AddTransient<UpdateSettingsPage>();
        builder.Services.AddTransient<UpdateSettingsViewModel>();

        builder.Services.AddSingleton<PositionDataBase>();

        builder.Services.AddSingleton(LogOperatorRetriever.Instance);

        return builder.Build();
    }
}
