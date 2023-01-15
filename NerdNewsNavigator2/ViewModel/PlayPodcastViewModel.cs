using CommunityToolkit.Mvvm.ComponentModel;

namespace NerdNewsNavigator2.ViewModel;

[QueryProperty("Url", "Url")]
public partial class PlayPodcastViewModel: ObservableObject
{
    public PlayPodcastViewModel() {  }
    
    [ObservableProperty]
    public string url;
}

