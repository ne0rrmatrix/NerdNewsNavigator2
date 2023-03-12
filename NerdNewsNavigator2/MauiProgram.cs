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
                    options.MinLevel = LogLevel.Trace;
                    options.MaxLevel = LogLevel.Critical;
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

#if WINDOWS
        builder.Services.AddSingleton<TabletPlayPodcastPage>();
#endif
#if IOS || ANDROID
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

        builder.Services.AddTransient<MostRecentShowsPage>();
        builder.Services.AddTransient<MostRecentShowsViewModel>();

        builder.Services.AddTransient<DownloadedShowPage>();
        builder.Services.AddTransient<DownloadedShowViewModel>();

        builder.Services.AddSingleton<PositionDataBase>();

        builder.Services.AddSingleton(LogOperatorRetriever.Instance);
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        return builder.Build();
    }
}
