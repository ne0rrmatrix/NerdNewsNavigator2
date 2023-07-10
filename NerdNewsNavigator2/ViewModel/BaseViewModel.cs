// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NerdNewsNavigator2.Extensions;

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// <c>BaseViewModel</c> is a <see cref="ViewModel"/> class that can be Inherited.
/// </summary>
public partial class BaseViewModel : ObservableObject, IRecipient<FullScreenItemMessage>
{
    #region Properties
    public delegate void DownloadCompletedEventHandler(object sender, DownloadEventArgs e);
    public delegate void DownloadChangedHandler();

    public event DownloadChangedHandler DownloadChanged;
    public DownloadNow Dnow { get; set; } = new();

    /// <summary>
    /// Gets the presented page.
    /// </summary>
    protected static Page CurrentPage
    {
        get
        {
            return PageExtensions.GetCurrentPage(Application.Current?.MainPage ?? throw new InvalidOperationException($"{nameof(Application.Current.MainPage)} cannot be null."));
        }
    }

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
        WeakReferenceMessenger.Default.Register<FullScreenItemMessage>(this);
        Dnow.DownloadCompleted += DownloadNow_DownloadCompletedAsync;
        DownloadChanged += () =>
        {
            Logger.LogInformation("NavBar closed");
        };
        if (Podcasts.Count > 0)
        {
            Logger.LogInformation("Got All Most Recent Shows");
        }
        ThreadPool.QueueUserWorkItem(async (state) => await GetFavoriteShows());
    }
    public void Receive(FullScreenItemMessage message)
    {
        var currentPage = BaseViewModel.CurrentPage;
        Debug.WriteLine($"Recieved message {message.Value}");
        if (message.Value)
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                NavigationPage.SetBackButtonTitle(currentPage, string.Empty);
                NavigationPage.SetHasBackButton(currentPage, false);
                Shell.SetFlyoutItemIsVisible(currentPage, false);
                Shell.SetNavBarIsVisible(currentPage, false);
                Shell.SetTabBarIsVisible(Shell.Current, false);
                NavigationPage.SetHasNavigationBar(currentPage, false);
                Shell.SetTabBarIsVisible(currentPage, false);
            });
        }
        else
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                NavigationPage.SetHasNavigationBar(currentPage, true);
                NavigationPage.SetHasBackButton(currentPage, true);
                Shell.SetFlyoutItemIsVisible(currentPage, true);
                Shell.SetNavBarIsVisible(currentPage, true);
                Shell.SetTabBarIsVisible(currentPage, true);
            });
        }
        WeakReferenceMessenger.Default.Reset();
        WeakReferenceMessenger.Default.Register<FullScreenItemMessage>(this);
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
            IsBusy = false;
            DownloadService.IsDownloading = false;
            Shell.SetNavBarIsVisible(Shell.Current.CurrentPage, false);
        });
        var item = IsMostRecent();
        WeakReferenceMessenger.Default.Send(new DownloadStatusMessage(false, item));
    }
    private async void DownloadNow_DownloadCompletedAsync(object sender, DownloadEventArgs e)
    {
        var show = e.Item;
        await SetProperties(show);
        _ = Task.Run(async () =>
        {
            await GetDownloadedShows();
        });
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
    public Show IsMostRecent()
    {
        if (MostRecentShows.ToList().Exists(x => x.IsDownloading))
        {
            return MostRecentShows.ToList().Find(x => x.IsDownloading);
        }
        if (Shows.ToList().Exists(x => x.IsDownloading))
        {
            return Shows.ToList().Find(x => x.IsDownloading);
        }
        return new Show();
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
        if (Shows.ToList().Exists(x => x.Url == url))
        {
            var showItem = Shows.ToList().Find(x => x.Url == url);
            showItem.IsDownloading = true;
            Shows[Shows.IndexOf(showItem)] = showItem;
            Dnow.Update(showItem);
        }
        else if (MostRecentShows.ToList().Exists(x => x.Url == url))
        {
            var showItem = MostRecentShows.ToList().Find(x => x.Url == url);
            showItem.IsDownloaded = false;
            MostRecentShows[MostRecentShows.IndexOf(showItem)] = showItem;
            Dnow.Update(showItem);
        }
        var item = IsMostRecent();
        WeakReferenceMessenger.Default.Send(new DownloadStatusMessage(true, item));
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
        Logger.LogInformation("Settings most recent shows");
        _ = Task.Run(async () => { await GetDownloadedShows(); });
        WeakReferenceMessenger.Default.Send(new DownloadItemMessage(true, download.Title));
        TriggerProgressChanged();
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
    public void GetShowsAsync(string url, bool getFirstOnly)
    {
        Shows.Clear();
        _ = Task.Run(async () =>
        {
            var downloads = await App.PositionData.GetAllDownloads();
            var temp = FeedService.GetShows(url, getFirstOnly);
            var item = BaseViewModel.RemoveDuplicates(temp);
            item.ForEach(x =>
            {
                if (downloads.Exists(y => y.Url == x.Url))
                {
                    x.IsDownloaded = true;
                    x.IsNotDownloaded = false;
                    x.IsDownloading = false;
                }
                Shows.Add(x);
            });
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
        IsBusy = true;
        if (MostRecentShows.Count > 0)
        {
            return;
        }
        while (App.AllShows.Count == 0)
        {
            Thread.Sleep(100);
        }
        App.AllShows.ForEach(MostRecentShows.Add);
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            IsBusy = false;

        });
        Logger.LogInformation("Got Most recent shows");
    }

    /// <summary>
    /// A method that sets <see cref="DownloadedShows"/> from the database.
    /// </summary>
    public async Task GetDownloadedShows()
    {
        DownloadedShows.Clear();
        var temp = await App.PositionData.GetAllDownloads();
        var item = BaseViewModel.RemoveDuplicates(temp);
        item.ForEach(DownloadedShows.Add);
        Logger.LogInformation("Add all downloads to All Shows list");
    }
    public static List<Show> RemoveDuplicates(List<Show> items)
    {
        List<Show> result = new();
        for (var i = 0; i < items.Count; i++)
        {
            var duplicate = false;
            for (var z = 0; z < i; z++)
            {
                if (items[z].Url == items[i].Url)
                {
                    duplicate = true;
                    break;
                }
            }
            if (!duplicate)
            {
                result.Add(items[i]);
            }
        }
        return result;
    }
    public static List<Download> RemoveDuplicates(List<Download> items)
    {
        List<Download> result = new();
        for (var i = 0; i < items.Count; i++)
        {
            var duplicate = false;
            for (var z = 0; z < i; z++)
            {
                if (items[z].Url == items[i].Url)
                {
                    duplicate = true;
                    break;
                }
            }
            if (!duplicate)
            {
                result.Add(items[i]);
            }
        }
        return result;
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
        }
        switch (DeviceInfo.Current.Idiom == DeviceIdiom.Desktop)
        {
            case true:
                return 3;
            case false:
                return 2;
        }
#pragma warning restore IDE0066
    }

    #endregion
}
