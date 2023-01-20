// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using Microsoft.Extensions.Logging;
using NerdNewsNavigator2.View;
using NerdNewsNavigator2.ViewModel;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.MediaPlayer;

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
        }).UseMauiCommunityToolkit().UseMauiCommunityToolkitMediaPlayer();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<PodcastPage>();
        builder.Services.AddSingleton<PodcastViewModel>();
        builder.Services.AddTransient<ShowPage>();
        builder.Services.AddTransient<ShowViewModel>();
        builder.Services.AddTransient<PlayPodcastPage>();
        builder.Services.AddTransient<PlayPodcastViewModel>();
        return builder.Build();
    }
}
