// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Maui.Devices;

namespace NerdNewsNavigator2.ViewModel;

public partial class SettingsViewModel : BaseViewModel
{
    public ObservableCollection<Podcast> Podcasts { get; set; } = new();
    public PodcastServices _podcastServices { get; set; }

    public SettingsViewModel(PodcastServices podcastServices)
    {
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
        _podcastServices = podcastServices;
        _ = GetUpdatedPodcasts();
    }
    private async Task GetUpdatedPodcasts()
    {
        var temp = await App.PositionData.GetAllPodcasts();
        foreach (var item in temp)
        {
            Podcasts.Add(item);
        }
        if (temp.Count == 0)
        {
            var items = _podcastServices.GetFromUrl().Result;
            foreach (var item in items)
            {
                Podcasts.Add(item);
                await App.PositionData.AddPodcast(item);
            }
        }
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
