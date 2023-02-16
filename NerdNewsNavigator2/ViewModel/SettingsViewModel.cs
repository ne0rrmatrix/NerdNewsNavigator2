﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

public partial class SettingsViewModel : BaseViewModel
{
    public SettingsViewModel(PodcastServices podcastServices)
    {
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
        _podcastServices = podcastServices;
        _ = GetUpdatedPodcasts();
    }

    [RelayCommand]
    public async Task Tap(string url)
    {
        await _podcastServices.Delete(url);
        foreach (var item in Podcasts)
        {
            if (item.Url == url)
            {
                if (Podcasts.Contains(item)) { Podcasts.Remove(item); }
                break;
            }
        }
    }
}
