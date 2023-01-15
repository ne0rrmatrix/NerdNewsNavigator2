

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NerdNewsNavigator2.View;

namespace NerdNewsNavigator2.ViewModel;

[QueryProperty("Url", "Url")]
public partial class PlayPodcastViewModel: ObservableObject
{
    public PlayPodcastViewModel() {  }
    
    [ObservableProperty]
    public string url;

   
}

