namespace NerdNewsNavigator2.View.Tablet;

public partial class TabletPodcastPage : ContentPage
{
   public TabletPodcastPage(TabletPodcastViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    private void OnQuit(object sender, EventArgs e)
    {
        Application.Current.Quit();
    }
    private void LivePage(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"{nameof(TabletLivePage)}");
    }
}
