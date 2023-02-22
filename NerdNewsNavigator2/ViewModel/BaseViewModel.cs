// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;
public partial class BaseViewModel : ObservableObject
{
    #region Properties
    public DisplayInfo MyMainDisplay { get; set; } = new();
    public ObservableCollection<Show> Shows { get; set; } = new();
    public ObservableCollection<Podcast> Podcasts { get; set; } = new();
    [ObservableProperty]
    public int _orientation;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public bool _isBusy;
    public bool IsNotBusy => !IsBusy;

    #endregion
    public BaseViewModel()
    {
    }
    #region Podcast data functions
    public async Task GetShows(string url)
    {
        Shows.Clear();
        var temp = await FeedService.GetShow(url);
        Shows = new ObservableCollection<Show>(temp);
    }
    public async Task GetUpdatedPodcasts()
    {
        Podcasts.Clear();
        OnPropertyChanged(nameof(IsBusy));
        IsBusy = true;
        var temp = await PodcastServices.GetUpdatedPodcasts();
        foreach (var item in temp)
        {
            Podcasts.Add(item);
        }

        if (temp.Count == 0)
        {
            var items = await PodcastServices.GetFromUrl();
            await PodcastServices.AddToDatabase(items);
            foreach (var item in items)
            {
                Podcasts.Add(item);
            }
        }
        IsBusy = false;
    }

    #endregion

    #region Display Functions
#nullable enable
    public void DeviceDisplay_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
#if IOS
        if (sender is null)
        {
            return;
        }
#endif
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
        else if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone && DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
            return 1;
        else if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone && DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Landscape)
            return 2;
        else if (DeviceInfo.Current.Idiom == DeviceIdiom.Tablet && DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
            return 2;
        else if (DeviceInfo.Current.Idiom == DeviceIdiom.Tablet && DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Landscape)
            return 3;
        else if (DeviceInfo.Current.Platform == DevicePlatform.iOS && DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
            return 2;
        else if (DeviceInfo.Current.Platform == DevicePlatform.iOS && DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Landscape)
            return 3;
        else return 1;
    }
    #endregion
}
