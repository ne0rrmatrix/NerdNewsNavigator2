// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits form <see cref="BaseViewModel"/> and manages showing <see cref="Show"/> information.
/// </summary>
[QueryProperty("Url", "Url")]
public partial class TabletShowViewModel : BaseViewModel
{
    #region Properties

    /// <summary>
    /// A Url <see cref="string"/> containing the <see cref="Show"/>
    /// </summary>
    public string Url
    {
        set
        {
            var decodedUrl = HttpUtility.UrlDecode(value);
            _ = GetShows(decodedUrl, false);
            OnPropertyChanged(nameof(Shows));
        }
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="TabletShowViewModel"/> class.
    /// </summary>
    public TabletShowViewModel()
    {
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
    }

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="TabletPlayPodcastPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    async Task Tap(string url) => await Shell.Current.GoToAsync($"{nameof(TabletPlayPodcastPage)}?Url={url}");
}
