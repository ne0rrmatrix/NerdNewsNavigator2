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
    /// An <see cref="ObservableCollection{T}"/> of <see cref="Show"/> managed by this class.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Favorites> _favoriteShows;

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of <see cref="Show"/> managed by this class.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Show> _shows;

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of downloaded <see cref="Download"/> managed by this class.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Download> _downloadedShows;

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of <see cref="Podcast"/> managed by this class.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Podcast> _podcasts;

    /// <summary>
    /// The <see cref="DisplayInfo"/> instance managed by this class.
    /// </summary>
    public DisplayInfo MyMainDisplay { get; set; }

    /// <summary>
    /// An <see cref="ILogger"/> instance managed by this class.
    /// </summary>
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(BaseViewModel));

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
    private bool _isRefreshing;
    #endregion
    public BaseViewModel(IConnectivity connectivity)
    {
        _connectivity = connectivity;
        _shows = new();
        _downloadedShows = new();
        _podcasts = new();
        _favoriteShows = new();
        MyMainDisplay = new();
        ThreadPool.QueueUserWorkItem(async (state) => await GetDownloadedShows());
        ThreadPool.QueueUserWorkItem(async (state) => await GetFavoriteShows());
        BindingBase.EnableCollectionSynchronization(Shows, null, ObservableCollectionCallback);
        BindingBase.EnableCollectionSynchronization(Podcasts, null, ObservableCollectionCallback);
        BindingBase.EnableCollectionSynchronization(DownloadedShows, null, ObservableCollectionCallback);
        BindingBase.EnableCollectionSynchronization(_favoriteShows, null, ObservableCollectionCallback);
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplayMainDisplayInfoChanged;
        Orientation = OnDeviceOrientationChange();
        if (!InternetConnected())
        {
            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
        }
    }
    #region Events
    public void DownloadStarted(object sender, DownloadEventArgs e)
    {
        MainThread.InvokeOnMainThreadAsync(() => Title = e.Title);
    }
    public void DownloadCancelled(object sender, DownloadEventArgs e)
    {
        MainThread.InvokeOnMainThreadAsync(() => Title = e.Title);
    }
    #endregion

    #region Relay Commands
    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="PodcastPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Play(string url)
    {
        Show show = new();
        if (DownloadedShows.Where(y => y.IsDownloaded).Any(x => x.Url == url))
        {
            var item = DownloadedShows.ToList().Find(x => x.Url == url);
            show.Title = item.Title;
            show.Url = item.FileName;
        }
        else
        {
            var item = Shows.First(x => x.Url == url);
            show.Url = item.Url;
            show.Title = item.Title;
        }
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}");
        App.OnVideoNavigated.Add(show);
    }
    #endregion

    #region Download Status Methods
    public void SetProperties(Show show)
    {
        var downloads = DownloadedShows.Any(x => x.Url == show.Url);
        if (downloads)
        {
            show.IsDownloaded = true;
            show.IsDownloading = false;
            show.IsNotDownloaded = false;
            return;
        }
        var currentDownload = App.DownloadService.Shows.Find(x => x.Url == show.Url);
        if (currentDownload is not null)
        {
            show.IsDownloaded = false;
            show.IsDownloading = true;
            show.IsNotDownloaded = false;
            return;
        }
        show.IsDownloading = false;
        show.IsNotDownloaded = true;
        show.IsDownloaded = false;
    }

    #endregion

    #region Update Shows
    /// <summary>
    /// <c>GetShows</c> is a <see cref="Task"/> that takes a <see cref="string"/> for Url and returns a <see cref="Show"/>
    /// </summary>
    /// <param name="url"></param> <see cref="string"/> URL of Twit tv Show
    /// <param name="getFirstOnly"><see cref="bool"/> Get first item only.</param>
    /// <returns><see cref="Show"/></returns>
    public void GetShowsAsync(string url, bool getFirstOnly)
    {
        if (!InternetConnected())
        {
            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
            return;
        }
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            Shows.Clear();
            var temp = FeedService.GetShows(url, getFirstOnly);
            temp.ForEach(SetProperties);
            Shows = new ObservableCollection<Show>(temp);
            _logger.Info("Got All Shows");
        });
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
        FavoriteShows = new ObservableCollection<Favorites>(temp);
        OnPropertyChanged(nameof(FavoriteShows));
        _logger.Info("Got all Favorite Shows");
    }

    /// <summary>
    /// A method that sets <see cref="DownloadedShows"/> from the database.
    /// </summary>
    public async Task GetDownloadedShows()
    {
        DownloadedShows.Clear();
        var temp = await App.PositionData.GetAllDownloads();
        temp.Where(x => !x.Deleted).ToList().ForEach(DownloadedShows.Add);
        _logger.Info("Add all downloads to All Shows list");
    }
    #endregion

    #region Update Podcasts
    /// <summary>
    /// <c>GetPodcasts</c> is a <see cref="Task"/> that sets <see cref="Podcasts"/> from either a Database or from the web.
    /// </summary>
    /// <returns></returns>
    public async Task GetPodcasts()
    {
        if (Podcasts.Count > 0)
        {
            return;
        }
        var temp = await App.PositionData.GetAllPodcasts();
        if (temp.Count > 0)
        {
            Podcasts = new ObservableCollection<Podcast>(temp);
            return;
        }
        var updates = PodcastServices.GetFromUrl();
        var sortedItems = updates.OrderBy(x => x.Title).ToList();
        Podcasts = new ObservableCollection<Podcast>(sortedItems);
        await PodcastServices.AddToDatabase(sortedItems);
    }
    public async Task UpdatePodcasts()
    {
        var temp = await App.PositionData.GetAllPodcasts();
        Podcasts = new ObservableCollection<Podcast>(temp);
    }
    #endregion

    #region Display Functions

#nullable enable

    /// <summary>
    /// <c>DeviceDisplay_MainDisplayInfoChanged</c> is a method that sets <see cref="Orientation"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void DeviceDisplayMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
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
    private void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
    {
        // `lock` ensures that only one thread access the collection at a time
        lock (collection)
        {
            accessMethod?.Invoke();
        }
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
}
