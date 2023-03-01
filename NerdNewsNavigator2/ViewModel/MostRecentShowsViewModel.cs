// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that manages the 
/// </summary>
public partial class MostRecentShowsViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of <see cref="MostRecentShowsViewModel"/>
    /// <paramref name="logger"/>
    /// </summary>
    public MostRecentShowsViewModel(ILogger<MostRecentShowsViewModel> logger)
    : base(logger)
    {
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
    }
    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="MostRecentShowsPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    private async Task Download(string url)
    {
        await Downloading(url, true);
    }

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="MostRecentShowsPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]

    public async Task Tap(string url) => await Shell.Current.GoToAsync($"{nameof(TabletPlayPodcastPage)}?Url={url}");
}
