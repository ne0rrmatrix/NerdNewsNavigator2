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
        DownloadChanged += () =>
        {
            Logger.LogInformation("NavBar closed");
        };
        if (Podcasts.Count > 0)
        {
            Logger.LogInformation("Got All Most Recent Shows");
        }
        ThreadPool.QueueUserWorkItem(async (state) => await GetDownloadedShows());
        ThreadPool.QueueUserWorkItem(async (state) => await GetFavoriteShows());
    }
    public void Receive(FullScreenItemMessage message)
    {
        var currentPage = BaseViewModel.CurrentPage;
        Logger.LogInformation("Recieved message {message}", message.Value);
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
        WeakReferenceMessenger.Default.Unregister<FullScreenItemMessage>(message);
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
            DownloadService.Progress = 0.00;
            DownloadProgress = string.Empty;
            Title = string.Empty;
#if WINDOWS
            Shell.SetNavBarIsVisible(Shell.Current.CurrentPage, false);
#endif
        });

    }
    public void SetProperties(Show show)
    {
        var shows = Shows.FirstOrDefault(x => x.Url == show.Url);
        var recent = MostRecentShows.FirstOrDefault(x => x.Url == show.Url);
        var allShow = App.AllShows.Find(x => x.Url == show.Url);
        var currentDownload = App.CurrenDownloads.Find(x => x.Url == show.Url);
        var downloads = DownloadedShows.ToList().Find(x => x.Url == show.Url);
        Logger.LogInformation("Set Properties received show: {show}: value: {value}", show.Title, show.IsDownloading);
        if (currentDownload is null)
        {
            show.IsDownloading = false;
            show.IsNotDownloaded = true;
            show.IsDownloaded = false;
        }
        if (currentDownload is not null)
        {
            show.IsDownloaded = false;
            show.IsDownloading = true;
            show.IsNotDownloaded = false;
            Logger.LogInformation("Finished setting properties for current downloads");
        }
        if (downloads is not null)
        {
            show.IsDownloaded = true;
            show.IsDownloading = false;
            show.IsNotDownloaded = false;
            Logger.LogInformation("Finished setting properties for downloaded show");
        }
        if (recent is not null)
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                MostRecentShows[MostRecentShows.IndexOf(recent)] = show;
            });
        }
        if (shows is not null)
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                Shows[Shows.IndexOf(shows)] = show;
            });
            Logger.LogInformation("Set Shows to - IsDownloading: {isDownlaoding}, IsNotdownloading: {IsNotDownloading}, IsDownloaded: {Isdownloaded} ", show.IsDownloading, show.IsNotDownloaded, show.IsDownloaded);
        }
        if (allShow is not null)
        {
            App.AllShows[App.AllShows.IndexOf(allShow)] = show;
        }
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
        var show = GetShowForDownload(url);
        Logger.LogInformation("Getting ready to send message about download starting");
        App.CurrenDownloads.Add(show);
        SetProperties(show);
        while (IsDownloading)
        {
            Thread.Sleep(5000);
            Logger.LogInformation("Waiting for download to finish");
        }

#if ANDROID || IOS
        var item = new Show();
        DownloadService.CancelDownload = false;
        if (Shows.ToList().Exists(x => x.Url == url))
        {
            item = Shows.ToList().Find(x => x.Url == url);
        }
        else if (MostRecentShows.ToList().Exists(x => x.Url == url))
        {
            item = MostRecentShows.ToList().Find(x => x.Url == url);
        }
        await NotificationService.CheckNotification();
        if (item is not null)
        {
            var requests = await NotificationService.NotificationRequests(item);
            NotificationService.AfterNotifications(requests);
        }
#endif
        IsDownloading = true;
        DownloadService.CancelDownload = false;
        await StartDownload(show);
    }
    public async Task StartDownload(Show show)
    {
#if WINDOWS || MACCATALYST
        _ = MainThread.InvokeOnMainThreadAsync(() => Shell.SetNavBarIsVisible(Shell.Current.CurrentPage, true));
#endif
        IsBusy = true;
        ThreadPool.QueueUserWorkItem(state => UpdatingDownloadAsync());
        Logger.LogInformation("Trying to start download of {URL}", show.Url);
        await ProcessDownloads(show);
        TriggerProgressChanged();
    }

    public Show GetShowForDownload(string url)
    {
        if (Shows.ToList().Exists(x => x.Url == url))
        {
            var showItem = Shows.ToList().Find(x => x.Url == url);
            return showItem;
        }
        else if (MostRecentShows.ToList().Exists(x => x.Url == url))
        {
            var showItem = MostRecentShows.ToList().Find(x => x.Url == url);
            return showItem;
        }
        return new Show();
    }
    public async Task ProcessDownloads(Show item)
    {
        Download download = new()
        {
            Title = item.Title,
            Url = item.Url,
            Image = item.Image,
            IsDownloaded = true,
            IsNotDownloaded = false,
            Deleted = false,
            PubDate = item.PubDate,
            Description = item.Description,
            FileName = DownloadService.GetFileName(item.Url)
        };
        DownloadService.IsDownloading = true;
        await DownloadService.DownloadFile(download.Url);
        await DownloadSuccess(download);
    }
    public void UpdatingDownloadAsync()
    {
        DownloadService.IsDownloading = true;
        while (DownloadService.IsDownloading)
        {
            DownloadProgress = DownloadService.Status;
            ProgressInfos = DownloadService.Progress;
            OnPropertyChanged(nameof(ProgressInfos));
            Title = DownloadProgress;
            Thread.Sleep(1000);
        }
        if (DownloadService.CancelDownload)
        {
            App.CurrenDownloads.Clear();
            Logger.LogInformation("Starting cleanup");
            Shows?.ToList().ForEach(x =>
            {
                if (x.IsDownloading)
                {
                    x.IsDownloading = false;
                    SetProperties(x);
                }
            });
            MostRecentShows?.ToList().ForEach(x =>
            {
                if (x.IsDownloading)
                {
                    x.IsDownloading = false;
                    SetProperties(x);
                }
            });
        }
    }
    public async Task DownloadSuccess(Download download)
    {
        IsDownloading = false;
        DownloadService.IsDownloading = false;
        var show = App.CurrenDownloads.Find(x => x.Title == download.Title);
        var filename = DownloadService.GetFileName(download.Url);
        var tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
        if (File.Exists(tempFile) && DownloadService.CancelDownload)
        {
            Logger.LogInformation("Deleting file from cancelled download: {FileName}", download.FileName);
            File.Delete(tempFile);
        }
        else
        {
            Logger.LogInformation("Downloaded file: {file}", download.FileName);
            App.CurrenDownloads.Clear();
            show.IsNotDownloaded = false;
            show.IsDownloaded = true;
            show.IsDownloading = false;
            Logger.LogInformation("Removed show from current downloads");
            await DownloadService.AddDownloadDatabase(download);
            DownloadedShows.Add(download);
            SetProperties(show);
            Shows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
            MostRecentShows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        }
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
        var temp = FeedService.GetShows(url, getFirstOnly);
        var item = BaseViewModel.RemoveDuplicates(temp);
        item.ForEach(Shows.Add);
        Shows.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        Shows.Where(x => App.CurrenDownloads.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        Logger.LogInformation("Got All Shows");
    }

    /// <summary>
    /// Method gets most recent episode from each podcast on twit.tv
    /// </summary>
    /// <returns></returns>
    public async Task GetMostRecent()
    {
        while (App.Started)
        {
            Thread.Sleep(100);
        }
        if (App.AllShows.Count == 0)
        {
            MostRecentShows.Clear();
            await App.GetMostRecent();
        }
        var item = App.AllShows.OrderBy(x => x.Title).ToList();
        item?.ForEach(MostRecentShows.Add);
        item.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        item.Where(x => App.CurrenDownloads.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
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
        if (Podcasts.Count > 0)
        {
            return;
        }
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
    }
    #endregion
}
