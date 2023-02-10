// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel.Phone;

[QueryProperty("Url", "Url")]
public partial class PhoneShowViewModel : ObservableObject
{
    #region Properties
    readonly PodcastServices _podcastService;
    public ObservableCollection<Show> Shows { get; set; } = new();
    public string Url
    {
        set
        {
            var decodedUrl = HttpUtility.UrlDecode(value);
            _ = GetShows(decodedUrl);
            OnPropertyChanged(nameof(Shows));
        }
    }
    #endregion
    public PhoneShowViewModel(PodcastServices podcastService)
    {
        _podcastService = podcastService;
    }
    #region Get the Show and Set Show List
    async Task GetShows(string url)
    {
        Shows.Clear();

        var temp = await PodcastServices.GetShow(url);
        Shows = new ObservableCollection<Show>(temp);
    }
    #endregion

    [RelayCommand]
    async Task Tap(string url) => await Shell.Current.GoToAsync($"{nameof(PhonePlayPodcastPage)}?Url={url}");
}
