using NerdNewsNavigator2.View;

namespace NerdNewsNavigator2;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(ShowPage), typeof(ShowPage));
		Routing.RegisterRoute(nameof(PlayPodcastPage), typeof(PlayPodcastPage)); 
    }
}
