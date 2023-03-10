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
    private string _url;
    /// <summary>
    /// A Url <see cref="string"/> containing the <see cref="Show"/>
    /// </summary>
    public string Url
    {
        get { return _url; }
        set
        {
            _url = value;
            var decodedUrl = HttpUtility.UrlDecode(value);
            _ = GetShows(decodedUrl, false);
            OnPropertyChanged(nameof(Shows));
        }
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="TabletShowViewModel"/> class.
    /// </summary>
    public TabletShowViewModel(ILogger<TabletShowViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
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
    public async Task Tap(string url) => await Shell.Current.GoToAsync($"{nameof(TabletPlayPodcastPage)}?Url={url}");

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="TabletShowPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Download(string url)
    {
        await Toast.Make("Added show to downloads.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        await Downloading(url, false);
    }
}
