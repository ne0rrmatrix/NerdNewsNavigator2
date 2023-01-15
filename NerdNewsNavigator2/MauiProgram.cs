using NerdNewsNavigator2.View;
using NerdNewsNavigator2.ViewModel;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.MediaView;
using Microsoft.Extensions.Logging;

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
        }).UseMauiCommunityToolkit().UseMauiCommunityToolkitMediaView();
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