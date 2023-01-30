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
}
