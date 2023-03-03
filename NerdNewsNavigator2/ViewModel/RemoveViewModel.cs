// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="RemovePage"/>
/// </summary>
public partial class RemoveViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveViewModel"/> instance.
    /// </summary>
    public RemoveViewModel(ILogger<RemoveViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
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
        var result = await PodcastServices.Delete(url);
        if (!result) { return; }
        var items = Podcasts.ToList();
        foreach (var item in from item in items
                             where item.Url == url
                             select item)
        {
            Podcasts.Remove(item);
        }
    }
}
