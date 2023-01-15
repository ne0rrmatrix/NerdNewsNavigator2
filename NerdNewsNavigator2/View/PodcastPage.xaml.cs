using NerdNewsNavigator2.ViewModel;

namespace NerdNewsNavigator2.View;

public partial class PodcastPage : ContentPage
{
    public PodcastPage(PodcastViewModel viewmodel)
    {
        InitializeComponent();
        BindingContext = viewmodel;
    }
}