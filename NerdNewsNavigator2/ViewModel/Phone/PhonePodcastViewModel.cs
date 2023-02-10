// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel.Phone;

public partial class PhonePodcastViewModel : ObservableObject
{
    #region Properties
    readonly TwitService _twitService;
    public ObservableCollection<Podcast> Podcasts { get; set; } = new();
    #endregion
    public PhonePodcastViewModel(TwitService twit)
    {
        this._twitService = twit;
        // _ = GetPodcasts();
        GetUpdatedPodcasts();
        OnPropertyChanged(nameof(Podcasts));
    }
    #region Get the Podcast and set the Podcast List
    async Task GetPodcasts()
    {
        var podcastList = await TwitService.GetListOfPodcasts();
        Podcasts.Clear();
        foreach (var item in podcastList)
        {
            Podcasts.Add(item);
        }
    }
    private void GetUpdatedPodcasts()
    {
        Podcasts.Clear();
        var podcastList = _twitService.Podcasts;
    }
    #endregion

    [RelayCommand]
    async Task Tap(string url)
    {
        var encodedUrl = HttpUtility.UrlEncode(url);
        await Shell.Current.GoToAsync($"{nameof(PhoneShowPage)}?Url={encodedUrl}");
    }
}
