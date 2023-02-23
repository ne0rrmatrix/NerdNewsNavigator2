// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

[QueryProperty("Url", "Url")]
public partial class TabletShowViewModel : BaseViewModel
{
    #region Properties
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
    public TabletShowViewModel()
    {
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
    }

    [RelayCommand]
    async Task Tap(string url) => await Shell.Current.GoToAsync($"{nameof(TabletPlayPodcastPage)}?Url={url}");
}
