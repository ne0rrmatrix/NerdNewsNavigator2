// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;
public partial class TabletPodcastViewModel : BaseViewModel
{
    public TabletPodcastViewModel(PodcastServices podcastServices)
    {
        PodServices = podcastServices;
        OnPropertyChanged(nameof(IsBusy));
        _ = GetUpdatedPodcasts();
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
    }

    [RelayCommand]
    async Task Tap(string url)
    {
        var encodedUrl = HttpUtility.UrlEncode(url);
        await Shell.Current.GoToAsync($"{nameof(TabletShowPage)}?Url={encodedUrl}");
    }
}
