// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel.Tablet;

[QueryProperty("Url", "Url")]
public partial class TabletShowViewModel : ObservableObject
{
    #region Properties
    readonly PodcastServices _podcastService;
    public ObservableCollection<Show> Shows { get; set; } = new();
    private DisplayInfo MyMainDisplay { get; set; } = new();

    [ObservableProperty]
    int _orientation;

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
#nullable enable
    private void DeviceDisplay_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
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
