// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;
/// <summary>
/// A class that manages displaying <see cref="Podcast"/> from twit.tv network.
/// </summary>
public partial class TabletPodcastViewModel : BaseViewModel
{
    readonly ILogger<TabletPodcastViewModel> _logger;
    /// <summary>
    /// Initializes a new instance of the <see cref="TabletPodcastViewModel"/> class.
    /// </summary>
    public TabletPodcastViewModel(ILogger<TabletPodcastViewModel> logger)
        : base(logger)
    {
        OnPropertyChanged(nameof(IsBusy));
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
        _logger = logger;
    }

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="TabletShowPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    async Task Tap(string url)
    {
        var encodedUrl = HttpUtility.UrlEncode(url);
        await Shell.Current.GoToAsync($"{nameof(TabletShowPage)}?Url={encodedUrl}");
    }
}
