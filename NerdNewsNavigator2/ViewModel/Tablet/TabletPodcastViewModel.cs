// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel.Tablet;

public partial class TabletPodcastViewModel : ObservableObject
{
    #region Properties
    public PodcastServices _podcastServices { get; set; } = new();
    public ObservableCollection<Podcast> Podcasts { get; set; } = new();
    private DisplayInfo MyMainDisplay { get; set; } = new();

    [ObservableProperty]
    int _orientation;
    #endregion
    public TabletPodcastViewModel(PodcastServices podcastServices)
    {
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
        _podcastServices = podcastServices;
        _ = GetUpdatedPodcasts();
    }

    #region Get the Podcast and set the Podcast List
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
    #endregion
#nullable enable
    private void DeviceDisplay_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        if (sender == null)
        {
            return;
        }
        MyMainDisplay = DeviceDisplay.Current.MainDisplayInfo;
        OnPropertyChanged(nameof(MyMainDisplay));
        Orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
    }
#nullable disable
    public static int OnDeviceOrientationChange()
    {
        if (DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait) { return 2; }
        else return 3;
    }

    [RelayCommand]
    async Task Tap(string url)
    {
        var encodedUrl = HttpUtility.UrlEncode(url);
        await Shell.Current.GoToAsync($"{nameof(TabletShowPage)}?Url={encodedUrl}");
    }
}
