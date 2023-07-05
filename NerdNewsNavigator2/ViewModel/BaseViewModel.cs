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
    public delegate void DownloadCompletedEventHandler(object sender, DownloadEventArgs e);
    public delegate void DownloadChangedHandler();

    public event DownloadChangedHandler DownloadChanged;
    public DownloadNow Dnow { get; set; } = new();

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of <see cref="Show"/> managed by this class.
    /// </summary>
    public ObservableCollection<Favorites> FavoriteShows { get; set; } = new();

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
    /// The <see cref="DisplayInfo"/> instance managed by this class.
    /// </summary>
    public DisplayInfo MyMainDisplay { get; set; } = new();

    /// <summary>
    /// An <see cref="ILogger{TCategoryName}"/> instance managed by this class.
    /// </summary>
    private ILogger<BaseViewModel> Logger { get; set; }

    /// <summary>
    /// an <see cref="IConnectivity"/> instance managed by this class.
    /// </summary>
    private readonly IConnectivity _connectivity;

    /// <summary>
    /// an <see cref="int"/> instance managed by this class. Used to set <see cref="Span"/> of <see cref="GridItemsLayout"/>
    /// </summary>
    [ObservableProperty]
    private int _orientation;

    [ObservableProperty]
    private string _title;

    /// <summary>
    /// an <see cref="int"/> instance managed by this class.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotDownloading))]
    private bool _isDownloading;

    /// <summary>
    /// an <see cref="int"/> instance managed by this class.
    /// </summary>
    public bool IsNotDownloading => !IsDownloading;

    /// <summary>
    /// an <see cref="int"/> instance managed by this class.
    /// </summary>
    [ObservableProperty]
    private string _downloadProgress;

    /// <summary>
    /// A <see cref="bool"/> instance managed by this class. 
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    /// <summary>
    /// A <see cref="bool"/> public property managed by this class.
    /// </summary>
    public bool IsNotBusy => !IsBusy;

    [ObservableProperty]
    private double _progressInfos = 0;

    #endregion
    public BaseViewModel(ILogger<BaseViewModel> logger, IConnectivity connectivity)
    {
        Logger = logger;
        _connectivity = connectivity;
        _downloadProgress = string.Empty;
        Dnow.DownloadCompleted += DownloadNow_DownloadCompletedAsync;
        DownloadChanged += () =>
        {
            Logger.LogInformation("NavBar closed");
            ThreadPool.QueueUserWorkItem(GetDownloadedShows);
        };
        if (Podcasts.Count > 0)
        {
            ThreadPool.QueueUserWorkItem(async (state) => await GetMostRecent());
            Logger.LogInformation("Got All Most Recent Shows");
        }
        ThreadPool.QueueUserWorkItem(GetDownloadedShows);
        ThreadPool.QueueUserWorkItem(async (state) => await GetFavoriteShows());
    }

    /// <summary>
    /// A method that checks if the internet is connected and returns a <see cref="bool"/> as answer.
    /// </summary>
    /// <returns></returns>
    public bool InternetConnected()
    {
        if (_connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            PodcastServices.IsConnected = true;
            return true;
        }
        else
        {
            PodcastServices.IsConnected = false;
            return false;
        }
    }
    /// <summary>
    /// Deletes file and removes it from database.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>

    [RelayCommand]
    public async Task Delete(string url)
    {
        var item = DownloadedShows.First(x => x.Url == url);
        if (item is null)
        {
            return;
        }
        var filename = DownloadService.GetFileName(item.Url);
        var tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
        if (File.Exists(tempFile))
        {
            File.Delete(tempFile);
            Logger.LogInformation("Deleted file {file}", tempFile);
            WeakReferenceMessenger.Default.Send(new DeletedItemMessage(true));
        }
        else
        {
            Logger.LogInformation("File {file} was not found in file system.", tempFile);
        }
        item.IsDownloaded = false;
        item.Deleted = true;
        item.IsNotDownloaded = true;
        await App.PositionData.UpdateDownload(item);
        DownloadedShows.Remove(item);
        SetDataAsync(url);
        Logger.LogInformation("Removed {file} from Downloaded Shows list.", url);
    }

    public void SetDataAsync(string url)
    {
        var allShow = App.AllShows.First(x => x.Url == url);
        allShow.IsDownloaded = false;
        allShow.IsNotDownloaded = true;
        allShow.IsDownloading = false;
        App.AllShows[App.AllShows.IndexOf(allShow)] = allShow;
        Dnow.Update(allShow);
    }
    #region Download Tasks
    private void TriggerProgressChanged()
    {
        DownloadChanged();
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            IsDownloading = false;
            IsBusy = false;
            DownloadService.IsDownloading = false;
            Shell.SetNavBarIsVisible(Shell.Current.CurrentPage, false);
        });
        WeakReferenceMessenger.Default.Send(new DownloadStatusMessage(false, App.AllShows.Find(x => x.IsDownloading)));
    }
    private async void DownloadNow_DownloadCompletedAsync(object sender, DownloadEventArgs e)
    {
        var show = e.Item;
        await SetProperties(show);
    }
    private async Task SetProperties(Show show)
    {
        var downloads = DownloadedShows.FirstOrDefault(x => x.Url == show.Url);
        var shows = Shows.FirstOrDefault(x => x.Url == show.Url);
        var recent = MostRecentShows.FirstOrDefault(x => x.Url == show.Url);
        if (downloads is not null)
        {
            show.IsDownloaded = true;
            show.IsDownloading = false;
            show.IsNotDownloaded = false;
        }
        if (recent is not null)
        {
            recent.IsDownloaded = show.IsDownloaded;
            recent.IsNotDownloaded = show.IsNotDownloaded;
            recent.IsDownloading = show.IsDownloading;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MostRecentShows[MostRecentShows.IndexOf(recent)] = recent;
            });
        }
        if (shows is not null)
        {
            shows.IsDownloaded = show.IsDownloaded;
            shows.IsNotDownloaded = show.IsNotDownloaded;
            shows.IsDownloading = show.IsDownloading;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Shows[Shows.IndexOf(shows)] = shows;
            });
        }
    }

    /// <summary>
    /// A Method that passes a Url to <see cref="DownloadService"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
#if ANDROID || IOS
    public async Task Download(string url)
#endif
#if WINDOWS || MACCATALYST
    public void Download(string url)
#endif
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Toast.Make("Added show to downloads.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        });

#if WINDOWS || MACCATALYST
        RunDownloads(url);
#endif
#if ANDROID || IOS
        DownloadService.CancelDownload = false;
        var item = Shows.First(x => x.Url == url);
        await NotificationService.CheckNotification();
        var requests = await NotificationService.NotificationRequests(item);
        NotificationService.AfterNotifications(requests);
        RunDownloads(url);
#endif
    }

    [RelayCommand]
    public static void Cancel()
    {
        DownloadService.CancelDownload = true;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Toast.Make("Download Cancelled", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        });
    }

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="VideoPlayerPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Play(string url)
    {
        var itemUrl = MostRecentShows.ToList().Find(x => x.Url == url);
        if (itemUrl is not null && itemUrl.IsDownloading)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Toast.Make("Video is Downloading. Please wait.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            });
            return;
        }
#if ANDROID || IOS || MACCATALYST
        var item = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DownloadService.GetFileName(url));
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}?Url={item}");
#endif
#if WINDOWS
        var item = "ms-appdata:///LocalCache/Local/" + DownloadService.GetFileName(url);
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}?Url={item}");
#endif
    }

    /// <summary>
    /// A method that download a Item to device.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task Downloading(string url)
    {
        if (!InternetConnected())
        {
            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
            return;
        }
        var item = App.AllShows.Find(x => x.Url == url);
        if (item != null)
        {
            item.IsDownloaded = false;
            item.IsNotDownloaded = true;
            item.IsDownloading = true;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                App.AllShows[App.AllShows.IndexOf(item)] = item;
            });
            WeakReferenceMessenger.Default.Send(new DownloadStatusMessage(true, item));
        }
        while (IsDownloading)
        {
            Thread.Sleep(5000);
            Logger.LogInformation("Waiting for download to finish");
        }
        DownloadService.CancelDownload = false;
        await StartDownload(url);
    }
    public async Task StartDownload(string url)
    {
#if WINDOWS || MACCATALYST
        _ = MainThread.InvokeOnMainThreadAsync(() => Shell.SetNavBarIsVisible(Shell.Current.CurrentPage, true));
#endif
        IsBusy = true;
        ThreadPool.QueueUserWorkItem(state => { UpdatingDownload(); });
        Logger.LogInformation("Trying to start download of {URL}", url);

        var item = App.AllShows.First(x => x.Url == url);
        await ProcessDownloads(item, url);
        TriggerProgressChanged();
    }
    public void RunDownloads(string url)
    {
        _ = Task.Run(async () =>
        {
            await Downloading(url);
        });
    }
    public async Task ProcessDownloads(Show item, string url)
    {
        Download download = new()
        {
            Title = item.Title,
            Url = url,
            Image = item.Image,
            IsDownloaded = true,
            IsNotDownloaded = false,
            Deleted = false,
            PubDate = item.PubDate,
            Description = item.Description,
            FileName = DownloadService.GetFileName(url)
        };
        Debug.WriteLine("Added Show to DownloadedShows");
        DownloadService.IsDownloading = true;
        var downloaded = await DownloadService.DownloadFile(download.Url);
        if (downloaded)
        {
            await DownloadSuccess(download);
        }
    }
    public void UpdatingDownload()
    {
        DownloadService.IsDownloading = true;
        IsDownloading = true;
        while (DownloadService.IsDownloading)
        {
            DownloadProgress = DownloadService.Status;
            ProgressInfos = DownloadService.Progress;
            OnPropertyChanged(nameof(ProgressInfos));
            Title = DownloadProgress;
            Thread.Sleep(1000);
        }
        TriggerProgressChanged();
    }
    public async Task DownloadSuccess(Download download)
    {
        Logger.LogInformation("Downloaded file: {file}", download.FileName);
        var result = await App.PositionData.GetAllDownloads();
        var item = result.Find(result => result.Title == download.Title);
        if (item != null)
        {
            await App.PositionData.DeleteDownload(item);
        }
        if (DownloadService.CancelDownload)
        {
            File.Delete(download.FileName);
            return;
        }

        await DownloadService.AddDownloadDatabase(download);
        IsDownloading = false;
        DownloadService.IsDownloading = false;
        DownloadedShows.Add(download);

        App.AllShows.ForEach(allShows =>
        {
            Debug.WriteLine("Settings most recent shows");
            allShows.IsDownloaded = false;
            allShows.IsNotDownloaded = true;
            allShows.IsDownloading = true;
            App.AllShows[App.AllShows.IndexOf(allShows)] = allShows;
        });
        WeakReferenceMessenger.Default.Send(new DownloadStatusMessage(true, App.AllShows.Find(x => x.IsDownloaded)));
        WeakReferenceMessenger.Default.Send(new DownloadItemMessage(true, download.Title));
    }
    #endregion

    #region Podcast data functions

    /// <summary>
    /// Method gets the <see cref="ObservableCollection{T}"/> of <see cref="Show"/> from the database.
    /// </summary>
    public async Task GetFavoriteShows()
    {
        FavoriteShows.Clear();
        var temp = await App.PositionData.GetAllFavorites();
        temp?.ForEach(FavoriteShows.Add);
        Logger.LogInformation("Got all Favorite Shows");
    }

    /// <summary>
    /// <c>GetShows</c> is a <see cref="Task"/> that takes a <see cref="string"/> for Url and returns a <see cref="Show"/>
    /// </summary>
    /// <param name="url"></param> <see cref="string"/> URL of Twit tv Show
    /// <param name="getFirstOnly"><see cref="bool"/> Get first item only.</param>
    /// <returns><see cref="Show"/></returns>
    public void GetShows(string url, bool getFirstOnly)
    {
        Shows.Clear();
        var temp = FeedService.GetShows(url, getFirstOnly);
        temp.ForEach(x =>
        {
            var item = App.AllShows.Find(y => y.Url == x.Url);
            if (item is not null)
            {
                x.IsDownloading = item.IsDownloading;
            }
            else { x.IsDownloading = false; }
            Shows.Add(x);
            App.AllShows.Add(x);
            Dnow.Update(x);
        });
        Logger.LogInformation("Got All Shows");
    }

    /// <summary>
    /// Method gets most recent episode from each podcast on twit.tv
    /// </summary>
    /// <returns></returns>
    public async Task GetMostRecent()
    {
        MostRecentShows.Clear();
        var temp = await App.PositionData.GetAllPodcasts();
        var item = temp.OrderBy(x => x.Title).ToList();
        item?.Where(x => !x.Deleted).ToList().ForEach(show =>
        {
            var item = FeedService.GetShows(show.Url, true);
            var app = App.AllShows.Find(y => y.Url == item[0].Url);
            if (app is null && item is not null)
            {
                App.AllShows.Add(item[0]);
            }
            if (item is not null)
            {
                MostRecentShows.Add(item[0]);
                if (item is not null && app is not null && app.IsDownloading)
                {
                    item[0].IsDownloading = app.IsDownloading;
                }
                Dnow.Update(item[0]);
            }
        });
        Logger.LogInformation("Got Most recent shows");
    }

    /// <summary>
    /// A method that sets <see cref="DownloadedShows"/> from the database.
    /// </summary>
    /// <param name="stateinfo"></param>
    public async void GetDownloadedShows(object stateinfo)
    {
        DownloadedShows.Clear();
        var temp = await App.PositionData.GetAllDownloads();
        temp?.Where(x => !x.Deleted).ToList().ForEach(DownloadedShows.Add);
    }

    #endregion

    #region Update Podcasts
    /// <summary>
    /// <c>GetUpdatedPodcasts</c> is a <see cref="Task"/> that sets <see cref="Podcasts"/> from either a Database or from the web.
    /// </summary>
    /// <returns></returns>
    public async Task GetUpdatedPodcasts()
    {
        Podcasts.Clear();

        var updates = await UpdateCheckAsync();
        var temp = await App.PositionData.GetAllPodcasts();
        if (!updates && temp.Count == 0)
        {
            await ProcessPodcasts();
        }
        else
        {
            var item = temp.OrderBy(x => x.Title).ToList();
            item?.Where(x => !x.Deleted).ToList().ForEach(Podcasts.Add);
        }
    }
    private async Task ProcessPodcasts()
    {
        var res = await PodcastServices.UpdatePodcast();
        Podcasts.Clear();

        // sort podcast alphabetically
        var orderPodcast = res.OrderBy(x => x.Title).ToList();

        orderPodcast.ForEach(Podcasts.Add);
        _ = Task.Run(async () =>
        {
            await GetFavoriteShows();
        });
    }
    private async Task<bool> UpdateCheckAsync()
    {
        var currentdate = DateTime.Now;
        var oldDate = Preferences.Default.Get("OldDate", DateTime.Now);
        Logger.LogInformation("Total day since last Update check for new Podcasts: {numberOfDays}", (currentdate - oldDate).Days.ToString());
        if ((currentdate - oldDate).Days <= 0)
        {
            Preferences.Default.Set("OldDate", DateTime.Now);
            Logger.LogInformation("Setting current date as Last Update Check");
            return false;
        }
        if ((oldDate - currentdate).Days > 30)
        {
            Logger.LogInformation("Last Update Check is over 30 days ago. Updating now.");
            Preferences.Default.Remove("OldDate");
            Preferences.Default.Set("OldDate", currentdate);
            var res = await PodcastServices.UpdatePodcast();
            Podcasts.Clear();
            App.AllShows.Clear();
            var item = res.OrderBy(x => x.Title).ToList();
            item.ForEach(Podcasts.Add);

            var fav = await PodcastServices.UpdateFavoritesAsync();
            FavoriteShows.Clear();
            fav.ForEach(FavoriteShows.Add);
            return true;
        }
        return false;
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
        if (DeviceDisplay.Current.MainDisplayInfo.Width <= 1920 && DeviceDisplay.Current.MainDisplayInfo.Width != 0 && DeviceInfo.Current.Platform == DevicePlatform.WinUI)
        {
            return 2;
        }
#pragma warning disable IDE0066
        switch (DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
        {
            case true:
                return DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? 1 : 2;
        }
        switch (DeviceInfo.Current.Idiom == DeviceIdiom.Tablet || DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            case true:
                return DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? 2 : 3;
            default:
                return 2;
        }
#pragma warning restore IDE0066
    }

    #endregion
}
