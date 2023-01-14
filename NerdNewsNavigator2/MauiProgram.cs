using Microsoft.Extensions.Logging;
using NerdNewsNavigator2.Model;
using NerdNewsNavigator2.View;
using NerdNewsNavigator2.ViewModel;
using CommunityToolkit.Maui;

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
        }).UseMauiCommunityToolkit();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<PodcastPage>();
        builder.Services.AddSingleton<PodcastViewModel>();
        builder.Services.AddTransient<ShowPage>();
        builder.Services.AddTransient<ShowViewModel>();
        return builder.Build();
    }
}