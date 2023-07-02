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
    public delegate void DownloadChangedHandler();

    public event DownloadChangedHandler DownloadChanged;
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
        DownloadChanged += () =>
        {
            Logger.LogInformation("NavBar closed");
            ThreadPool.QueueUserWorkItem(GetDownloadedShows);
        };
        ThreadPool.QueueUserWorkItem(GetDownloadedShows);
        ThreadPool.QueueUserWorkItem(GetFavoriteShows);
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
    #region Download Tasks
    private void TriggerProgressChanged()
    {
        DownloadChanged();
        MainThread.InvokeOnMainThreadAsync(() =>
       {
           IsDownloading = false;
           DownloadService.IsDownloading = false;
           Shell.SetNavBarIsVisible(Shell.Current.CurrentPage, false);
       });
    }

    /// <summary>
    /// A method that download a show to device.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task Downloading(string url)
    {
        SetProperties(App.AllShows.First(x => x.Url == url), url, true);
        if (!InternetConnected())
        {
            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
            return;
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
        else
        {
            SetProperties(item, url, false);
        }
        App.SafeShutdown = true;
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
        WeakReferenceMessenger.Default.Send(new DownloadItemMessage(true, download.Title));
    }
    private void SetProperties(Show item, string url, bool isDownloading)
    {
        App.SafeShutdown = false;
        if (MostRecentShows.Count > 0)
        {
            var recent = MostRecentShows.First(x => x.Url == url);
            recent.IsDownloading = isDownloading;
            recent.IsNotDownloaded = !isDownloading;
            recent.IsDownloaded = isDownloading;
            MostRecentShows[MostRecentShows.IndexOf(recent)] = recent;
        }
        if (Shows.Count > 0)
        {
            var show = Shows.First(x => x.Url == url);
            show.IsDownloading = isDownloading;
            show.IsDownloaded = isDownloading;
            show.IsNotDownloaded = !isDownloading;
            Shows[Shows.IndexOf(show)] = show;
        }
        if (item is not null)
        {
            item.IsDownloaded = isDownloading;
            item.IsNotDownloaded = !isDownloading;
            item.IsDownloading = isDownloading;
            App.AllShows[App.AllShows.IndexOf(item)] = item;
        }
    }
    #endregion

    #region Podcast data functions

    /// <summary>
    /// Method gets the <see cref="ObservableCollection{T}"/> of <see cref="Show"/> from the database.
    /// </summary>
    /// <param name="stateinfo"></param>
    public async void GetFavoriteShows(object stateinfo)
    {
        FavoriteShows.Clear();
        var temp = await App.PositionData.GetAllFavorites();
        temp?.ForEach(FavoriteShows.Add);
    }
    public async Task GetAllShows()
    {
        if (App.AllShows.Count > 0)
        {
            return;
        }
        var items = await App.PositionData.GetAllPodcasts();
        App.AllShows.Clear();
        while (items.Count == 0)
        {
            Logger.LogInformation("Sleeping 1000ms, waiting for Podcasts list to be available");
            Thread.Sleep(1000);
            items = await App.PositionData.GetAllPodcasts();
        }
        items.ToList().ForEach(x =>
        {
            var item = FeedService.GetShows(x.Url, false);
            item.ForEach(App.AllShows.Add);
        });
        Logger.LogInformation("Got all Shows");
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
            var showIsdownloading = App.AllShows.First(y => y.Url == x.Url);
            var downloaded = DownloadedShows.Any(y => y.Url == x.Url);
            if (showIsdownloading is not null && showIsdownloading.IsDownloading && !downloaded)
            {
                x.IsDownloading = true;
                x.IsNotDownloaded = false;
                x.IsDownloaded = true;
            }
            else
            {
                x.IsNotDownloaded = true;
                x.IsDownloaded = false;
            }
            if (downloaded)
            {
                x.IsDownloading = false;
                x.IsDownloaded = true;
                x.IsNotDownloaded = false;
            }
            Shows.Add(x);
        });
    }

    /// <summary>
    /// Method gets most recent episode from each podcast on twit.tv
    /// </summary>
    /// <returns></returns>
    public async Task GetMostRecent()
    {
        MostRecentShows.Clear();
        var temp = await App.PositionData.GetAllPodcasts();
        while (temp.Count == 0)
        {
            Logger.LogInformation("Sleeping 1000ms, waiting for Podcasts list to be available");
            Thread.Sleep(1000);
            temp = await App.PositionData.GetAllPodcasts();
        }
        var item = temp.OrderBy(x => x.Title).ToList();
        item?.Where(x => !x.Deleted).ToList().ForEach(show =>
            {
                var item = FeedService.GetShows(show.Url, true);
                var downloaded = DownloadedShows.Any(y => y.Url == item[0].Url);
                var showIsdownloading = App.AllShows.First(y => y.Url == item[0].Url);
                if (showIsdownloading is not null && showIsdownloading.IsDownloading && !downloaded)
                {
                    item[0].IsDownloading = true;
                    item[0].IsNotDownloaded = false;
                    item[0].IsDownloaded = true;
                }
                else
                {
                    item[0].IsNotDownloaded = true;
                    item[0].IsDownloaded = false;
                }
                if (downloaded)
                {
                    item[0].IsNotDownloaded = false;
                    item[0].IsDownloaded = true;
                    item[0].IsDownloading = false;
                }
                MostRecentShows.Add(item[0]);
            });
        Logger.LogInformation("Got Most recent shows");
        App.SafeShutdown = true;
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
        App.SafeShutdown = false;
        Podcasts.Clear();
        var updates = await UpdateCheckAsync();
        if (updates)
        {
            return;
        }
        var temp = await App.PositionData.GetAllPodcasts();
        if (temp.Count == 0)
        {
            var res = await PodcastServices.UpdatePodcast();
            Podcasts.Clear();

            // sort podcast alphabetically
            var orderPodcast = res.OrderBy(x => x.Title).ToList();

            orderPodcast.ForEach(Podcasts.Add);
            var fav = await PodcastServices.UpdateFavoritesAsync();
            FavoriteShows.Clear();
            fav.ForEach(FavoriteShows.Add);
            _ = Task.Run(async () =>
            {
                await GetAllShows();
                await GetMostRecent();
                App.SafeShutdown = true;
            });
            return;
        }
        var item = temp.OrderBy(x => x.Title).ToList();
        item?.Where(x => !x.Deleted).ToList().ForEach(Podcasts.Add);
        _ = Task.Run(async () =>
        {
            await GetAllShows();
            await GetMostRecent();
            App.SafeShutdown = true;
        });
    }

    private async Task<bool> UpdateCheckAsync()
    {
        var currentdate = DateTime.Now;
        var oldDate = Preferences.Default.Get("OldDate", DateTime.Now);
        Logger.LogInformation("Total day since last Update check for new Podcasts: {numberOfDays}", (currentdate - oldDate).Days.ToString());
        if ((currentdate - oldDate).TotalDays <= 0)
        {
            Preferences.Default.Set("OldDate", DateTime.Now);
            Logger.LogInformation("Setting current date as Last Update Check");
        }
        if ((oldDate - currentdate).Days > 30)
        {
            Logger.LogInformation("Last Update Check is over 30 days ago. Updating now.");
            Preferences.Default.Remove("OldDate");
            Preferences.Default.Set("OldDate", currentdate);
            var res = await PodcastServices.UpdatePodcast();
            Podcasts.Clear();
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
