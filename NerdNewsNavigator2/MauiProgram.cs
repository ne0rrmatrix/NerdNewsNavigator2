using Microsoft.Extensions.Logging;
using NerdNewsNavigator2.Model;
using NerdNewsNavigator2.View;
using NerdNewsNavigator2.ViewModel;

namespace NerdNewsNavigator2;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<Podcast>();
        builder.Services.AddSingleton<Show>();
        builder.Services.AddSingleton<PodcastViewModel>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<PodcastPage>();
        return builder.Build();
	}
}
