// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel.Tablet;

[QueryProperty("Url", "Url")]
public partial class TabletShowViewModel : ObservableObject
{
    #region Properties
    readonly TwitService _twitService;
    [ObservableProperty]
    private string _orientation;
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
    public TabletShowViewModel(TwitService twitService)
    {
        _twitService = twitService;
        this._orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(_orientation));
    }
    public string OnDeviceOrientationChange()
    {
        string Orientation = string.Empty;
        if (DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait) { Orientation = "3"; }
        else Orientation = "4";
        System.Diagnostics.Debug.WriteLine("Screen orientation: " + Orientation);
        return Orientation;
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
