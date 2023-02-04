// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel.Tablet;

public partial class TabletPodcastViewModel : ObservableObject
{
    #region Properties
    readonly TwitService _twitService;
    private DisplayInfo MyMainDisplay { get; set; } = new();
    public ObservableCollection<Podcast> Podcasts { get; set; } = new();

    [ObservableProperty]
    int _orientation;
    #endregion
    public TabletPodcastViewModel(TwitService twit)
    {
        this._twitService = twit;
        _ = GetPodcasts();
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
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
#nullable enable
    private void DeviceDisplay_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        if (sender == null)
        {
            return;
        }
        MyMainDisplay = DeviceDisplay.Current.MainDisplayInfo;
        OnPropertyChanged(nameof(MyMainDisplay));
        Orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
    }
#nullable disable
    public static int OnDeviceOrientationChange()
    {
        if (DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait) { return 2; }
        else return 3;
    }

    [RelayCommand]
    async Task Tap(string url)
    {
        var encodedUrl = HttpUtility.UrlEncode(url);
        await Shell.Current.GoToAsync($"{nameof(TabletShowPage)}?Url={encodedUrl}");
    }
}
