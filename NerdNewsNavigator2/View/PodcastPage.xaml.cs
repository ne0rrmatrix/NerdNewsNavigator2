using NerdNewsNavigator2.ViewModel;

namespace NerdNewsNavigator2.View;

public partial class PodcastPage : ContentPage
{
    public PodcastPage()
    {
        InitializeComponent();
        this.BindingContext = new PodcastViewModel();
    }
}