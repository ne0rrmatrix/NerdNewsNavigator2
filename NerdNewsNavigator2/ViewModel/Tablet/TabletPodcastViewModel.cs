// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel.Tablet;

public partial class TabletPodcastViewModel : ObservableObject
{
    #region Properties
    readonly TwitService _twitService;
    [ObservableProperty]
    private string _orientation;
    public ObservableCollection<Podcast> Podcasts { get; set; } = new();
    #endregion
    public TabletPodcastViewModel(TwitService twit)
    {
        this._twitService = twit;
        _ = GetPodcasts();
        this._orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(_orientation));
    }

    #region Get the Podcast and set the Podcast List
    async Task GetPodcasts()
    {
        var podcastList = await TwitService.GetListOfPodcasts();
        foreach (var item in podcastList)
        {
            var temp = await Task.FromResult(FeedService.GetFeed(item));
            Podcasts.Add(temp);
        }
    }
    #endregion
    public string OnDeviceOrientationChange()
    {
        string Orientation = string.Empty;
        if (DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait) { Orientation = "4"; }
        else Orientation = "3";
        System.Diagnostics.Debug.WriteLine("Screen orientation: " + Orientation);
        return Orientation;
    }

    [RelayCommand]
    async Task Tap(string url)
    {
        var encodedUrl = HttpUtility.UrlEncode(url);
        await Shell.Current.GoToAsync($"{nameof(TabletShowPage)}?Url={encodedUrl}");
    }
}
