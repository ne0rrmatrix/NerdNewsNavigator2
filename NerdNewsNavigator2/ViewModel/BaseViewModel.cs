﻿// Licensed to the .NET Foundation under one or more agreements.
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
    /// The <see cref="bool"/> instance managed by this class.
    /// </summary>
    private bool _setFullScreen = false;
    /// <summary>
    /// The <see cref="bool"/> public property managed by this class.
    /// </summary>
    public bool SetFullScreen
    {
        get => _setFullScreen;
        set => SetProperty(ref _setFullScreen, value);
    }
    /// <summary>
    /// The <see cref="MauiWinUIWindow"/> public property managed by this class.
    /// </summary>
#if WINDOWS
    public static MauiWinUIWindow CurrentWindow { get; set; }
#endif
    /// <summary>
    /// The <see cref="DisplayInfo"/> instance managed by this class.
    /// </summary>
    public DisplayInfo MyMainDisplay { get; set; } = new();

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of <see cref="Show"/> managed by this class.
    /// </summary>
    public ObservableCollection<Show> Shows { get; set; } = new();

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
    /// an <see cref="int"/> instance managed by this class. Used to set <see cref="Span"/> of <see cref="GridItemsLayout"/>
    /// </summary>
    private int _orientation;
    /// <summary>
    /// An <see cref="int"/> public property managed by this class. Used to set <see cref="Span"/> of <see cref="GridItemsLayout"/>
    /// </summary>
    public int Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }

    /// <summary>
    /// A <see cref="bool"/> instance managed by this class. 
    /// </summary>
    private bool _isBusy;

    /// <summary>
    /// A <see cref="bool"/> public property managed by this class.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }
    }

    /// <summary>
    /// A <see cref="bool"/> public property managed by this class.
    /// </summary>
    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// An <see cref="ILogger{TCategoryName}"/> instance managed by this class.
    /// </summary>
    ILogger<BaseViewModel> Logger { get; set; }

    /// <summary>
    /// an <see cref="IConnectivity"/> instance managed by this class.
    /// </summary>
    private readonly IConnectivity _connectivity;

    #endregion
    public BaseViewModel(ILogger<BaseViewModel> logger, IConnectivity connectivity)
    {
        Logger = logger;
        this._connectivity = connectivity;
        ThreadPool.QueueUserWorkItem(GetDownloadedShows);
#if WINDOWS || ANDROID
        ThreadPool.QueueUserWorkItem(GetMostRecent);
#endif
#if IOS || MACCATALYST
        _ = GetMostRecent();
#endif
    }

    /// <summary>
    /// A Method that detects mouse movment on <see cref="LivePage"/>
    /// </summary>
    [RelayCommand]
    public async Task Moved()
    {
        if (!IsBusy)
        {
            await SetIsBusy();
        }
    }
    /// <summary>
    /// A method that sets <see cref="SetFullScreen"/> on and off with a timer.
    /// </summary>
    /// <returns></returns>
    public async Task Waiting()
    {
        if (!SetFullScreen)
        {
            SetFullScreen = true;

            await Task.Delay(4000);
            SetFullScreen = false;
        }
    }
    /// <summary>
    /// A method that converts <see cref="TimeSpan"/> into a usable <see cref="string"/> for displaying position in <see cref="MediaElement"/>
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static string TimeConverter(TimeSpan time)
    {
        var interval = new TimeSpan(time.Hours, time.Minutes, time.Seconds);
        return (interval).ToString();
    }

    /// <summary>
    /// A method that sets whether the <see cref="Page"/> is Full screen or not.
    /// </summary>
    /// <returns></returns>
    public async Task SetIsBusy()
    {
        SetFullScreen = true;
        await Task.Delay(4000);
        SetFullScreen = false;
    }
    /// <summary>
    /// A method that checks if the internet is connected and returns a <see cref="bool"/> as answer.
    /// </summary>
    /// <returns></returns>
    public bool InternetConnected()
    {
        if (_connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// A method that download a show to device.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="mostRecent"></param>
    /// <returns></returns>
    public async Task Downloading(string url, bool mostRecent)
    {
        Logger.LogInformation("Trying to start download of {URL}", url);
        IsBusy = true;
        List<Show> list;
        if (mostRecent) { list = MostRecentShows.ToList(); }
        else { list = Shows.ToList(); }

        foreach (var item in from item in list
                             where item.Url == url
                             select item)
        {
            Logger.LogInformation("Found match!");
            Download download = new()
            {
                Title = item.Title,
                Url = url,
                Image = item.Image,
                PubDate = item.PubDate,
                Description = item.Description,
                FileName = DownloadService.GetFileName(url)
            };
            var downloaded = await DownloadService.DownloadShow(download);
            if (!downloaded)
            {
                IsBusy = false;
                WeakReferenceMessenger.Default.Send(new DownloadItemMessage(false));

            }
            else if (downloaded)
            {
                Logger.LogInformation("Downloaded file: {file}", download.FileName);
                var result = await App.PositionData.GetAllDownloads();
                foreach (var show in result)
                {
                    if (show.Title == download.Title)
                    {
                        await App.PositionData.DeleteDownload(show);
                    }
                }
                await DownloadService.AddDownloadDatabase(download);
                WeakReferenceMessenger.Default.Send(new DownloadItemMessage(true));
                IsBusy = false;
            }
        }
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
        if (InternetConnected())
        {
            var temp = await FeedService.GetShows(url, getFirstOnly);
            Shows = new ObservableCollection<Show>(temp);
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
        }
    }
#if WINDOWS || ANDROID
    /// <summary>
    /// Method gets most recent episode from each podcast on twit.tv
    /// </summary>
    /// <param name="stateinfo"></param>
    /// <returns></returns>
    public async void GetMostRecent(object stateinfo)
    {
        MostRecentShows.Clear();
        await GetUpdatedPodcasts();
        if (InternetConnected())
        {
            foreach (var show in Podcasts.ToList())
            {
                var item = await FeedService.GetShows(show.Url, true);
                MostRecentShows.Add(item.First());
            }
        }
        else if (!InternetConnected())
        {

            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
        }
    }
#endif

#if IOS || MACCATALYST
    /// <summary>
    /// Method gets most recent episode from each podcast on twit.tv
    /// </summary>
    /// <returns></returns>
    public async Task GetMostRecent()
    {
        MostRecentShows.Clear();
        await GetUpdatedPodcasts();
        if (InternetConnected())
        {
            foreach (var show in Podcasts.ToList())
            {
                var item = await FeedService.GetShows(show.Url, true);
                MostRecentShows.Add(item.First());
            }
        }
        else if (!InternetConnected())
        {

            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
        }
    }
#endif

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
        if ((temp is null || temp.Count == 0) && InternetConnected())
        {
            var items = await PodcastServices.GetFromUrl();
            await PodcastServices.AddToDatabase(items);
            foreach (var item in items)
            {
                Podcasts.Add(item);
            }
            IsBusy = false;
        }
        else if (temp is not null)
        {
            foreach (var item in temp)
            {
                Podcasts.Add(item);
            }
            IsBusy = false;
            if (!InternetConnected())
            {
                WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
            }
        }
    }
    /// <summary>
    /// A method that sets <see cref="DownloadedShows"/> from the database.
    /// </summary>
    /// <param name="stateinfo"></param>
    public async void GetDownloadedShows(object stateinfo)
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
