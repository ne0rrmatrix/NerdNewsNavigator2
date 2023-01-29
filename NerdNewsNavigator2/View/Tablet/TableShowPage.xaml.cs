namespace NerdNewsNavigator2.View.Tablet;

public partial class TabletShowPage : ContentPage
{
    public TabletShowPage(TabletShowViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync($"{nameof(TabletPodcastPage)}");
        return true;
    }
    private void LivePage(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"{nameof(TabletLivePage)}");
    }
    private void GoBack(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"{nameof(TabletPodcastPage)}");
    }
    private void OnQuit(object sender, EventArgs e)
    {
        Application.Current.Quit();
    }
}
