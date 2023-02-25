// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Maui.Storage;

namespace NerdNewsNavigator2.ViewModel;

public partial class DownloadedShowViewModel : BaseViewModel
{
    #region Properties

    readonly ILogger<DownloadedShowViewModel> _logger;
    /// <summary>
    /// Intilializes an instance of <see cref="DownloadedShowViewModel"/>
    /// <paramref name="logger"/>
    /// </summary>

    #endregion

    public DownloadedShowViewModel(ILogger<DownloadedShowViewModel> logger)
        : base(logger)
    {
        _logger = logger;
        _logger.LogInformation("DownloadedShowViewModel started.");
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(DownloadedShows));
    }

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="DownloadPlayPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    async Task Tap(string url)
    {
        string item = "ms-appdata:///LocalCache/Local/" + url;
        _logger.LogInformation("Url being passed is: {name}", item);
        await Shell.Current.GoToAsync($"{nameof(DownloadPlayPage)}?Url={item}");
    }
}

