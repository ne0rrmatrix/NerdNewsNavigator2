namespace NerdNewsNavigator2.View.Tablet;

public partial class TabletLivePage : ContentPage
{
    public TabletLivePage(TabletLiveViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync($"{nameof(TabletPodcastPage)}");
        return true;
    }
}
