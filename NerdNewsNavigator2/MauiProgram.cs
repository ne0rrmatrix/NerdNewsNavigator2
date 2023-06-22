// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
            // Font added to address issue: https://github.com/dotnet/maui/issues/13239
            // fonts.AddFont("OpenSans-Medium.ttf", "OpenSansMedium"); !! Still throws error !!
            // Alias set to match error string
            fonts.AddFont("OpenSans-Medium.ttf", "sans-serif-medium");
        }).UseMauiCommunityToolkit().UseMauiCommunityToolkitMediaElement()
#if ANDROID || IOS
        .UseLocalNotification();
#else
        ;
#endif
        #region Logging
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
        #endregion
        #region Services

        builder.Services.AddSingleton<AndroidPermissions>();
        builder.Services.AddSingleton<BaseViewModel>();

        builder.Services.AddTransient<PodcastPage>();
        builder.Services.AddTransient<PodcastViewModel>();

        builder.Services.AddTransient<ShowPage>();
        builder.Services.AddTransient<ShowViewModel>();

#if WINDOWS
        builder.Services.AddSingleton<VideoPlayerPage>();
#endif
#if IOS || ANDROID || MACCATALYST
        builder.Services.AddTransient<VideoPlayerPage>();
#endif
        builder.Services.AddTransient<VideoPlayerViewModel>();

        builder.Services.AddTransient<LivePage>();
        builder.Services.AddTransient<LiveViewModel>();

        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<SettingsViewModel>();

        builder.Services.AddTransient<EditPage>();
        builder.Services.AddTransient<EditViewModel>();

        builder.Services.AddTransient<ResetAllSettingsPage>();
        builder.Services.AddTransient<ResetAllSettingsViewModel>();

        builder.Services.AddTransient<MostRecentShowsPage>();
        builder.Services.AddTransient<MostRecentShowsViewModel>();

        builder.Services.AddTransient<DownloadedShowPage>();
        builder.Services.AddTransient<DownloadedShowViewModel>();

        builder.Services.AddSingleton<PositionDataBase>();

        builder.Services.AddSingleton(LogOperatorRetriever.Instance);
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();
        return builder.Build();
        #endregion
    }
}
