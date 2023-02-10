// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel.Desktop;
public partial class DesktopPodcastViewModel : ObservableObject
{
    #region Properties
    public TwitService _twitService { get; set; }
    public ObservableCollection<Podcast> Podcasts { get; set; } = new();
    #endregion
    public DesktopPodcastViewModel(TwitService twit)
    {
        _twitService = twit;
        GetUpdatedPodcasts();
    }

    #region Get the Podcast and set the Podcast List
    private void GetUpdatedPodcasts()
    {
        Podcasts.Clear();
        var temp = _twitService.Podcasts;
        foreach (var item in temp)
        {
            Podcasts.Add(item);
        }
    }

    #endregion

    [RelayCommand]
    async Task Tap(string url)
    {
        var encodedUrl = HttpUtility.UrlEncode(url);
        await Shell.Current.GoToAsync($"{nameof(DesktopShowPage)}?Url={encodedUrl}");
    }
}
