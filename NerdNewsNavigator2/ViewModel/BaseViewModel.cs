﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;
public partial class BaseViewModel : ObservableObject
{
    public DisplayInfo MyMainDisplay { get; set; } = new();
    public PositionServices Services { get; set; } = new();
    public PodcastServices _podcastServices { get; set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public bool _isBusy;
    public bool IsNotBusy => !IsBusy;

    [ObservableProperty]
    public int _orientation;
    public ObservableCollection<Podcast> Podcasts { get; set; } = new();

    public BaseViewModel()
    {
    }

    public async Task GetUpdatedPodcasts()
    {
        Podcasts.Clear();
        IsBusy = true;
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
        IsBusy = false;
    }
    public async Task AddPodcastsToDatabase()
    {
        foreach (var item in Podcasts)
        {
            await App.PositionData.AddPodcast(item);
        }
    }
#nullable enable
    public void DeviceDisplay_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        MyMainDisplay = DeviceDisplay.Current.MainDisplayInfo;
        OnPropertyChanged(nameof(MyMainDisplay));
        Orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
    }
#nullable disable
    public static int OnDeviceOrientationChange()
    {
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            return 3;
        else if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
            return 1;
        else if (DeviceInfo.Current.Idiom == DeviceIdiom.Tablet && DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
            return 2;
        else if (DeviceInfo.Current.Idiom == DeviceIdiom.Tablet && DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Landscape) { return 3; }
        else return 1;
    }
}
