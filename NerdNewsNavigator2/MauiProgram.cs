// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NerdNewsNavigator2.IViews;

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
        }).UseMauiCommunityToolkit().UseMauiCommunityToolkitMediaPlayer().UseViewServices();
#if DEBUG
        builder.Logging.AddDebug();
#endif
#if ANDROID || IOS

        builder.Services.AddSingleton<IPodcastPage, MobilePodcastPage>();
        builder.Services.AddTransient<IPlayLivePage, MobileLiveViewModel>();
        builder.Services.AddSingleton<IPodcastPage, MobilePodcastViewModel>();
        builder.Services.AddTransient<IShowPage, MobileShowPage>();
        builder.Services.AddTransient<IPlayPodcastPage, MobilePlayPodcastPage>();
        builder.Services.AddTransient<IShowPage, MobileShowViewModel>();
        builder.Services.AddTransient<IPlayPodcastPage, MobilePlayPodcastViewModel>();
        builder.Services.AddTransient<IPlayLivePage, MobileLivePage>();
#else
        builder.Services.AddSingleton<IPodcastPage, DesktopPodcastPage>();
        builder.Services.AddSingleton<IPodcastPage, DesktopPodcastViewModel>();
        builder.Services.AddSingleton<IShowPage,DesktopShowPage>();
        builder.Services.AddTransient<IShowPage, DesktopShowViewModel>();
        builder.Services.AddSingleton<IPlayPodcastPage, DesktopPlayPodcastPage>();
        builder.Services.AddTransient<IPlayPodcastPage, DesktopPlayPodcastViewModel>();
        builder.Services.AddSingleton<IPlayLivePage, DesktopLivePage>();
        builder.Services.AddTransient<IPlayLivePage, DesktopLiveViewModel>();
#endif

        builder.Services.AddSingleton<TwitService>();
        builder.Services.AddSingleton<FeedService>();
        return builder.Build();
    }
}
