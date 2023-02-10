// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel.Desktop;
public partial class DesktopPodcastViewModel : ObservableObject
{
    #region Properties
    public PodcastServices _podcastServices { get; set; } = new();
    public ObservableCollection<Podcast> Podcasts { get; set; } = new();
    #endregion
    public DesktopPodcastViewModel(PodcastServices podcastServices)
    {
        _podcastServices = podcastServices;
        _ = GetUpdatedPodcasts();
        
    }

    #region Get the Podcast and set the Podcast List
    private async Task GetUpdatedPodcasts()
    {
        var temp = await App.PositionData.GetAllPodcasts();
        foreach (var item in temp)
        {
            Podcasts.Add(item);
        }
        if (temp.Count == 0)
        {
            var items = _podcastServices.GetFromUrl().Result;
            foreach (var item in items)
            {
                Podcasts.Add(item);
                await App.PositionData.AddPodcast(item);
            }
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
