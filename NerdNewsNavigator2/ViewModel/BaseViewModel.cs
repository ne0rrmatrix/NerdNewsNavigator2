// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// <c>BaseViewModel</c> is a <see cref="ViewModel"/> class that can be Inherited.
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    #region Properties

    /// <summary>
    /// The status of <see cref="ThreadPool.QueueUserWorkItem(WaitCallback)"/> return value.
    /// </summary>
    public bool Started { get; set; }
    /// <summary>
    /// The <see cref="DisplayInfo"/> instance managed by this class.
    /// </summary>
    public DisplayInfo MyMainDisplay { get; set; } = new();

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of <see cref="Show"/> managed by this class.
    /// </summary>
    public ObservableCollection<Show> Shows { get; set; } = new();
    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of <see cref="Show"/> managed by this class.
    /// </summary>
    public ObservableCollection<Show> AllShows { get; set; } = new();

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of most recent <see cref="Show"/> managed by this class.
    /// </summary>
    public ObservableCollection<Show> MostRecentShows { get; set; } = new();

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of downloaded <see cref="Download"/> managed by this class.
    /// </summary>
    public ObservableCollection<Download> DownloadedShows { get; set; } = new();

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of <see cref="Podcast"/> managed by this class.
    /// </summary>
    public ObservableCollection<Podcast> Podcasts { get; set; } = new();

    /// <summary>
    /// An <see cref="int"/> instance managed by this class. Used to set <see cref="Span"/> of <see cref="GridItemsLayout"/>
    /// </summary>
    [ObservableProperty]
    public int _orientation;

    /// <summary>
    /// A <see cref="bool"/> instance managed by this class.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public bool _isBusy;

    /// <summary>
    /// A <see cref="bool"/> instance managed by this class.
    /// </summary>
    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// A <see cref="bool"/> instance managed by this class.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotDownloading))]
    public bool _isDownloading;

    /// <summary>
    /// A <see cref="bool"/> instance managed by this class.
    /// </summary>
    public bool IsNotDownloading => !IsDownloading;
    /// <summary>
    /// An <see cref="ILogger{TCategoryName}"/> instance managed by this class.
    /// </summary>
    ILogger<BaseViewModel> _logger { get; set; }

    #endregion
    public BaseViewModel(ILogger<BaseViewModel> logger)
    {
        _logger = logger;
        Started = false;
        _logger.LogInformation("Starting background tasks.");
        Started = ThreadPool.QueueUserWorkItem(GetMostRecent);
        _logger.LogInformation("Started GetMostRecent: {started}", Started);
        Started = ThreadPool.QueueUserWorkItem(GetDownloadedShows);
        _logger.LogInformation("Started GetDownloadedShows: {started}", Started);
        Started = ThreadPool.QueueUserWorkItem(GetAllShows);
        _logger.LogInformation("Started GeteAllShows: {started}", Started);
    }

    #region Podcast data functions

    /// <summary>
    /// <c>GetShows</c> is a <see cref="Task"/> that takes a <see cref="string"/> for <see cref="Show.Url"/> and returns a <see cref="Show"/>
    /// </summary>
    /// <param name="url"></param> <see cref="string"/> URL of Twit tv Show
    /// <param name="getFirstOnly"><see cref="bool"/> Get first item only.</param>
    /// <returns><see cref="Show"/></returns>
    public async Task GetShows(string url, bool getFirstOnly)
    {
        Shows.Clear();
        var temp = await FeedService.GetShows(url, getFirstOnly);
        Shows = new ObservableCollection<Show>(temp);
    }

    /// <summary>
    /// Method gets most recent episode from each podcast on twit.tv
    /// </summary>
    /// <param name="stateinfo"></param>
    /// <returns></returns>
    public async void GetMostRecent(Object stateinfo)
    {
        Shows.Clear();
        await GetUpdatedPodcasts();
        foreach (var show in Podcasts.ToList())
        {
            var item = await FeedService.GetShows(show.Url, true);
            MostRecentShows.Add(item.First());
        }
    }

    /// <summary>
    /// <c>GetUpdatedPodcasts</c> is a <see cref="Task"/> that sets <see cref="Podcasts"/> from either a Database or from the web.
    /// </summary>
    /// <returns></returns>
    public async Task GetUpdatedPodcasts()
    {
        Podcasts.Clear();
        OnPropertyChanged(nameof(IsBusy));
        IsBusy = true;
        var temp = await PodcastServices.GetUpdatedPodcasts();
        if (temp is null || temp.Count == 0)
        {
            var items = await PodcastServices.GetFromUrl();
            await PodcastServices.AddToDatabase(items);
            foreach (var item in items)
            {
                Podcasts.Add(item);
            }
            IsBusy = false;
        }
        else
        {
            foreach (var item in temp)
            {
                Podcasts.Add(item);
            }
            IsBusy = false;
        }
    }

    /// <summary>
    /// Method dowloads All Show data and adds it to <see cref="AllShows"/>
    /// </summary>
    /// <param name="stateinfo"></param>
    /// <returns></returns>
    public async void GetAllShows(Object stateinfo)
    {
        Thread.Sleep(1000);
        foreach (var podcast in Podcasts.ToList())
        {
            var shows = await PodcastServices.GetShow(podcast.Url, false);
            foreach (var show in shows)
            {
                AllShows.Add(show);
            }
        }
        _logger.LogInformation("Downloaded all show data.");
    }

    /// <summary>
    /// Method gets downloaded shows on device from Database.
    /// </summary>
    /// <param name="stateinfo"></param>
    /// <returns></returns>
    public async void GetDownloadedShows(Object stateinfo)
    {
        DownloadedShows.Clear();
        var temp = await App.PositionData.GetAllDownloads();
        if (temp is not null)
        {
            foreach (var item in temp)
            {
                DownloadedShows.Add(item);
            }
        }
    }

    #endregion

    #region Display Functions

#nullable enable

    /// <summary>
    /// <c>DeviceDisplay_MainDisplayInfoChanged</c> is a method that sets <see cref="Orientation"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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

    /// <summary>
    /// <c>OnDeviceOrientation</c> is a method that is used to set <see cref="Span"/> of <see cref="GridItemsLayout"/>
    /// </summary>
    /// <returns><see cref="int"/></returns>
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
