using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NerdNewsNavigator2.View;

namespace NerdNewsNavigator2.ViewModel;

[QueryProperty("Url", "Url")]
public partial class PlayPodcastViewModel: ObservableObject
{
    #region Properties
    [ObservableProperty]
    public string url;
    #endregion
    
    [RelayCommand]
    async Task SwipeGesture_Left_Show(string Url) => await Shell.Current.GoToAsync($"{nameof(ShowPage)}?Url={Url}");
}

