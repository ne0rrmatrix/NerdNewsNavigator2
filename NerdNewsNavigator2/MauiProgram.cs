// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MetroLog.Targets;
using NerdNewsNavigator2.Devices;
using NerdNewsNavigator2.Interface;
using Woka;

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
        }).UseMauiCommunityToolkit().UseMauiCommunityToolkitMediaElement().ConfigureWorkarounds()
#if ANDROID || IOS
        .UseLocalNotification();
#else
        ;
#endif
        #region Logging
        var config = new LoggingConfiguration();

#if RELEASE
        config.AddTarget(
            LogLevel.Info,
            LogLevel.Fatal,
            new StreamingFileTarget(Path.Combine(
                FileSystem.CacheDirectory,
                "MetroLogs"), retainDays: 2));
#else
        // Will write logs to the Debug output
        config.AddTarget(
            LogLevel.Trace,
            LogLevel.Fatal,
            new TraceTarget());
#endif

        // will write logs to the console output (Logcat for android)
        config.AddTarget(
            LogLevel.Info,
            LogLevel.Fatal,
            new ConsoleTarget());

        config.AddTarget(
            LogLevel.Info,
            LogLevel.Fatal,
            new MemoryTarget(2048));

        LoggerFactory.Initialize(config);
        #endregion

        #region Services

        builder.Services.AddTransient<PodcastPage>();
        builder.Services.AddTransient<PodcastViewModel>();

        builder.Services.AddTransient<ShowPage>();
        builder.Services.AddTransient<ShowViewModel>();

        builder.Services.AddTransient<VideoPlayerPage>();
        builder.Services.AddTransient<VideoPlayerViewModel>();

        builder.Services.AddTransient<LivePage>();
        builder.Services.AddTransient<LiveViewModel>();

        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<SettingsViewModel>();

        builder.Services.AddTransient<EditPage>();
        builder.Services.AddTransient<EditViewModel>();

        builder.Services.AddTransient<ResetAllSettingsPage>();
        builder.Services.AddTransient<ResetAllSettingsViewModel>();

        builder.Services.AddTransient<DownloadedShowPage>();
        builder.Services.AddTransient<DownloadedShowViewModel>();

        builder.Services.AddSingleton<BaseViewModel>();
        builder.Services.AddSingleton<SharedViewModel>();

        builder.Services.AddSingleton<CurrentDownloads>();
        builder.Services.AddSingleton<DeletedItemService>();
        builder.Services.AddSingleton<VideoOnNavigated>();
        builder.Services.AddSingleton<NotificationService>();
        builder.Services.AddSingleton<DownloadService>();
        builder.Services.AddSingleton<AutoDownloadService>();

        builder.Services.AddSingleton<PositionDataBase>();
        builder.Services.AddSingleton<AndroidPermissions>();
        builder.Services.AddSingleton(LogOperatorRetriever.Instance);
        builder.Services.AddSingleton<IDeviceServices, DeviceServices>();
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();
        builder.Services.AddSingleton<MessagingService>();
        return builder.Build();
        #endregion
    }
}
