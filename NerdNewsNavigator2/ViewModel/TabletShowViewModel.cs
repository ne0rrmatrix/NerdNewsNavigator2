// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

[QueryProperty("Url", "Url")]
public partial class TabletShowViewModel : BaseViewModel
{
    #region Properties
    readonly PodcastServices _podcastService;
    public ObservableCollection<Show> Shows { get; set; } = new();

    public string Url
    {
        set
        {
            var decodedUrl = HttpUtility.UrlDecode(value);
            GetShows(decodedUrl);
            OnPropertyChanged(nameof(Shows));
        }
    }
    #endregion
    public TabletShowViewModel(PodcastServices podcastService)
    {
        _podcastService = podcastService;
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
    }

    #region Get the Show and Set Show List
    void GetShows(string url)
    {
        Shows.Clear();
        var temp = Task.FromResult(FeedService.GetShow(url)).Result;
        Shows = new ObservableCollection<Show>(temp);
    }
    #endregion

    [RelayCommand]
    async Task Tap(string url) => await Shell.Current.GoToAsync($"{nameof(TabletPlayPodcastPage)}?Url={url}");
}
