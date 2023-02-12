// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

public partial class TabletPodcastViewModel : BaseViewModel
{
    #region Properties
    public PodcastServices _podcastServices { get; set; } = new();
    public ObservableCollection<Podcast> Podcasts { get; set; } = new();

    #endregion
    public TabletPodcastViewModel(PodcastServices podcastServices)
    {
        _podcastServices = podcastServices;
        _ = GetUpdatedPodcasts();
        OnPropertyChanged(nameof(Podcasts));
        OnPropertyChanged(nameof(IsBusy));
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
    }

    #region Get the Podcast and set the Podcast List
    private async Task GetUpdatedPodcasts()
    {
        try
        {
            Podcasts.Clear();
            IsBusy = true;
            var temp = await App.PositionData.GetAllPodcasts();
            foreach (var item in temp)
            {
                Podcasts.Add(item);
            }
            if (Podcasts.Count == 0)
            {
                var items = _podcastServices.GetFromUrl().Result;
                foreach (var item in items)
                {
                    Podcasts.Add(item);
                }
                _ = AddPodcastsToDatabase();
            }
            IsBusy = false;
        }
        catch { }
    }
    private async Task AddPodcastsToDatabase()
    {
        foreach (var item in Podcasts)
        {
            await App.PositionData.AddPodcast(item);
        }
    }
    #endregion

    [RelayCommand]
    async Task Tap(string url)
    {
        var encodedUrl = HttpUtility.UrlEncode(url);
        await Shell.Current.GoToAsync($"{nameof(TabletShowPage)}?Url={encodedUrl}");
    }
}
