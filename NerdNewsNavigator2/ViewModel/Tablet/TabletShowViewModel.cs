// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel.Tablet;

[QueryProperty("Url", "Url")]
public partial class TabletShowViewModel : ObservableObject
{
    #region Properties
    readonly TwitService _twitService;
    public ObservableCollection<Show> Shows { get; set; } = new();
    private DisplayInfo MyMainDisplay { get; set; } = new();

    [ObservableProperty]
    int _orientation;
   
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
    public TabletShowViewModel(TwitService twitService)
    {
        _twitService = twitService;
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
    }
    private void DeviceDisplay_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        MyMainDisplay = DeviceDisplay.Current.MainDisplayInfo;
        OnPropertyChanged(nameof(MyMainDisplay));
        Orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
    }
    public static int OnDeviceOrientationChange()
    {
        if (DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait) { return 2; }
        else return 3;
    }
    #region Get the Show and Set Show List
    async Task GetShows(string url)
    {
        Shows.Clear();

        var temp = await TwitService.GetShow(url);
        Shows = new ObservableCollection<Show>(temp);
    }
    #endregion

    [RelayCommand]
    async Task Tap(string url) => await Shell.Current.GoToAsync($"{nameof(TabletPlayPodcastPage)}?Url={url}");
}
