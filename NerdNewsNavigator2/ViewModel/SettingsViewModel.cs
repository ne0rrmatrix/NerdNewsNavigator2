// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits form <see cref="BaseViewModel"/> and manages <see cref="SettingsPage"/>
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    ILogger<SettingsViewModel> Logger { get; set; }
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    public SettingsViewModel(ILogger<SettingsViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        Logger = logger;
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        Orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
        _ = GetUpdatedPodcasts();
    }

    /// <summary>
    /// Method Deletes a <see cref="Podcast"/> from <see cref="List{T}"/> <see cref="Podcast"/> in <see cref="BaseViewModel"/>
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Tap(string url)
    {
        await PodcastServices.Delete(url);
        var item = Podcasts.First(x => x.Url == url);
        if (item is null)
        {
            Logger.LogInformation("Podcast {Item} could not be deleted. It was not found in list of Podcasts", url);
            return;
        }
        Podcasts.Remove(item);
        Logger.LogInformation("Podcast {Item} was removed form List of Podcasts", item.Url);
    }
}
