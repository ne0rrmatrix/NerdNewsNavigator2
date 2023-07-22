// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Maui.BindableProperty.Generator.Core;

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
    public ObservableCollection<Favorites> FavoriteShows { get; set; } = new();

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of <see cref="Show"/> managed by this class.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Show> _shows;

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of most recent <see cref="Show"/> managed by this class.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Show> _mostRecentShows;

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of downloaded <see cref="Download"/> managed by this class.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Download> _downloadedShows;

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
    public static string CancelUrl { get; set; }
    public static bool CancelDownload { get; set; }
    public PodcastServices PodServices { get; set; } = new();
    #endregion
    public BaseViewModel(ILogger<BaseViewModel> logger, IConnectivity connectivity)
    {
        Logger = logger;
        _connectivity = connectivity;
        _shows = new();
        _downloadProgress = string.Empty;
        _downloadedShows = new();
        _mostRecentShows = new();
        ThreadPool.QueueUserWorkItem(async (state) => await GetDownloadedShows());
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

    #region Download Tasks

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
    /// A method that sets <see cref="DownloadedShows"/> from the database.
    /// </summary>
    public async Task GetDownloadedShows()
    {
        DownloadedShows.Clear();
        var temp = await App.PositionData.GetAllDownloads();
        temp.Where(x => !x.Deleted).ToList().ForEach(DownloadedShows.Add);
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
