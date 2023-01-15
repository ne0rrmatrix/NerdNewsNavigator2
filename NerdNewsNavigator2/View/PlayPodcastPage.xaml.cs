using NerdNewsNavigator2.ViewModel;

namespace NerdNewsNavigator2.View;

public partial class PlayPodcastPage : ContentPage
{
	public PlayPodcastPage(PlayPodcastViewModel viewmodel)
	{
		InitializeComponent();
		BindingContext = viewmodel;
	}
}