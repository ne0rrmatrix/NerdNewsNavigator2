namespace NerdNewsNavigator2.View.Tablet;

public partial class TabletPodcastPage : ContentPage
{
   public TabletPodcastPage(TabletPodcastViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
