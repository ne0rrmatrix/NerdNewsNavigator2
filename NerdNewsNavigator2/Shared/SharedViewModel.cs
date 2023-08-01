// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Shared;

[QueryProperty("Url", "Url")]
public partial class SharedViewModel : BaseViewModel
{
    #region Properties
    /// <summary>
    /// An <see cref="ILogger"/> instance managed by this class.
    /// </summary>
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(SharedViewModel));
    /// <summary>
    /// A private <see cref="string"/> that contains a Url for <see cref="Show"/>
    /// </summary>
    [ObservableProperty]
    private string _url;

    [ObservableProperty]
    private bool _isRefreshing;

    #endregion
    public SharedViewModel(IConnectivity connectivity) : base(connectivity)
    {
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        Orientation = OnDeviceOrientationChange();

        if (!InternetConnected())
        {
            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
        }
    }
    partial void OnUrlChanged(string oldValue, string newValue)
    {
        _logger.Info("Show Url changed. Updating Shows");
        var decodedUrl = HttpUtility.UrlDecode(newValue);
#if WINDOWS || MACCATALYST || ANDROID
        ThreadPool.QueueUserWorkItem(state => GetShowsAsync(decodedUrl, false));
#endif
#if IOS
        GetShowsAsync(decodedUrl, false);
#endif
    }

    #region Events

    public void DonwnloadCancelled(object sender, DownloadEventArgs e)
    {
        App.Downloads.DownloadCancelled -= DonwnloadCancelled;
        ThreadPool.QueueUserWorkItem(state =>
        {
            Thread.Sleep(1000);
            App.Downloads.DownloadCancelled += DonwnloadCancelled;
            if (e.Shows.Count > 0)
            {
                _logger.Info("Starting Second Download");
                App.Downloads.Start(e.Shows[0]);
            }
        });
    }
    public void UpdateOnCancel(object sender, Primitives.DownloadEventArgs e)
    {
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            Shows?.ToList().ForEach(SetProperties);
            MostRecentShows?.ToList().ForEach(SetProperties);
            _logger.Info("update for shows not downlaoding anymore");
        });
    }
    public void OnNavigated(object sender, Primitives.NavigationEventArgs e)
    {
        Shows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        Shows?.Where(x => App.Downloads.Shows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        MostRecentShows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        MostRecentShows?.Where(x => App.Downloads.Shows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
    }
    public void DownloadStarted(object sender, DownloadEventArgs e)
    {
        if (e.Status is null || e.Shows.Count == 0)
        {
            return;
        }
        Title = e.Status;
    }
    public void UpdateShows()
    {
        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            IsBusy = false;
            Title = string.Empty;
            DownloadProgress = string.Empty;
            MostRecentShows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(item =>
            {
                var number = MostRecentShows.IndexOf(item);
                MostRecentShows[number].IsDownloaded = true;
                MostRecentShows[number].IsDownloading = false;
                MostRecentShows[number].IsNotDownloaded = false;
                OnPropertyChanged(nameof(MostRecentShows));
            });
            Shows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(item =>
            {
                var number = Shows.IndexOf(item);
                Shows[number].IsDownloaded = true;
                Shows[number].IsDownloading = false;
                Shows[number].IsNotDownloaded = false;
                OnPropertyChanged(nameof(Shows));
            });
        });
    }
    public async void DownloadCompleted(object sender, DownloadEventArgs e)
    {
#if ANDROID || IOS
        App.Downloads.Notify.StopNotifications();
#endif
        App.Downloads.DownloadStarted -= DownloadStarted;
        App.Downloads.DownloadCancelled -= DonwnloadCancelled;
        App.Downloads.DownloadFinished -= DownloadCompleted;
        await GetDownloadedShows();
        _logger.Info("Shared View model - Downloaded event firing");
        UpdateShows();
        if (e.Shows.Count > 0)
        {
            _logger.Info("Starting next show: {Title}", e.Shows[0].Title);
#if ANDROID || IOS
            App.Downloads.Notify.StartNotifications();
#endif
            App.Downloads.DownloadStarted += DownloadStarted;
            App.Downloads.DownloadCancelled += DonwnloadCancelled;
            App.Downloads.DownloadFinished += DownloadCompleted;
            App.Downloads.Start(e.Shows[0]);
        }
    }
    #endregion

    #region Relay Commands
    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="PodcastPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Tap(string url)
    {
        if (DownloadedShows.Where(y => y.IsDownloaded).ToList().Exists(x => x.Url == url))
        {
            var download = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DownloadService.GetFileName(url));
            await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}");
            var item = DownloadedShows.ToList().Find(x => x.Url == url);
            Show show = new()
            {
                Url = download,
                Title = item.Title,
            };
            App.OnVideoNavigated.Add(show);
            return;
        }
        var temp = Shows.ToList().Find(x => x.Url == url) ?? MostRecentShows.ToList().Find(y => y.Url == url) ?? throw new NullReferenceException();
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}");
        App.OnVideoNavigated.Add(temp);
    }

    /// <summary>
    /// Deletes file and removes it from database.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Delete(string url)
    {
        var item = DownloadedShows.FirstOrDefault(x => x.Url == url);
        if (item is null)
        {
            return;
        }
        var filename = DownloadService.GetFileName(item.Url);
        var tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
        if (File.Exists(tempFile))
        {
            File.Delete(tempFile);
            _logger.Info($"Deleted file {tempFile}");
            WeakReferenceMessenger.Default.Send(new DeletedItemMessage(true));
        }
        else
        {
            _logger.Info($"File {tempFile} was not found in file system.");
        }
        item.IsDownloaded = false;
        item.Deleted = true;
        item.IsNotDownloaded = true;
        await App.PositionData.UpdateDownload(item);
        DownloadedShows.Remove(item);
        _logger.Info($"Removed {url} from Downloaded Shows list.");
        _logger.Info("Failed to find a show to update");
        MostRecentShows.Clear();
        await GetMostRecent();
    }

    /// <summary>
    /// A Method that passes a Url to <see cref="DownloadService"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public void Download(string url)
    {
#if ANDROID
        _ = EditViewModel.CheckAndRequestForeGroundPermission();
#endif
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Toast.Make("Added show to downloads.", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
        });
        var item = GetShowForDownload(url);
        item.IsDownloading = true;
        if (Shows.ToList().Exists(x => x.Url == item.Url))
        {
            var number = Shows.IndexOf(item);
            Shows[number].IsDownloaded = false;
            Shows[number].IsDownloading = true;
            Shows[number].IsNotDownloaded = false;
            OnPropertyChanged(nameof(Shows));
        }
        if (MostRecentShows.ToList().Exists(x => x.Url == item.Url))
        {
            var number = MostRecentShows.IndexOf(item);
            MostRecentShows[number].IsDownloaded = false;
            MostRecentShows[number].IsDownloading = true;
            MostRecentShows[number].IsNotDownloaded = false;
            OnPropertyChanged(nameof(MostRecentShows));
        }
        if (App.Downloads.Shows.Count == 0)
        {
            _logger.Info($"Current download count is: {App.Downloads.Shows.Count}");
            App.Downloads.DownloadStarted += DownloadStarted;
            App.Downloads.DownloadCancelled += DonwnloadCancelled;
            App.Downloads.DownloadFinished += DownloadCompleted;
        }
        App.Downloads.Add(item);
        App.Downloads.Start(item);
    }

    [RelayCommand]
    public void Cancel(string url)
    {
        Title = string.Empty;
        var item = App.Downloads.Cancel(url);
        if (item is null)
        {
            _logger.Info("show was null");
            return;
        }
        var show = Shows.ToList().Find(x => x.Url == item.Url);
        if (show is not null)
        {
            var number = Shows.IndexOf(show);
            Shows[number].IsDownloaded = false;
            Shows[number].IsDownloading = false;
            Shows[number].IsNotDownloaded = true;
            OnPropertyChanged(nameof(Shows));
        }
        var mostRecent = MostRecentShows.ToList().Find(x => x.Url == item.Url);
        if (mostRecent is not null)
        {
            var number = MostRecentShows.IndexOf(mostRecent);
            MostRecentShows[number].IsDownloaded = false;
            MostRecentShows[number].IsDownloading = false;
            MostRecentShows[number].IsNotDownloaded = true;
            OnPropertyChanged(nameof(MostRecentShows));
        }
        DownloadProgress = string.Empty;
    }
    #endregion

    #region Download Status Methods
    public void SetProperties(Show show)
    {
        var shows = Shows.FirstOrDefault(x => x.Url == show.Url);
        var recent = MostRecentShows.FirstOrDefault(x => x.Url == show.Url);
        var currentDownload = App.Downloads.Shows.Find(x => x.Url == show.Url);
        var downloads = DownloadedShows.ToList().Find(x => x.Url == show.Url);
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
        }
        if (downloads is not null)
        {
            show.IsDownloaded = true;
            show.IsDownloading = false;
            show.IsNotDownloaded = false;
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
        }
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
        Shows.Clear();
        var temp = FeedService.GetShows(url, getFirstOnly);
        var item = BaseViewModel.RemoveDuplicates(temp);
        item.ForEach(Shows.Add);
        _logger.Info("Got All Shows");
        Shows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        Shows?.Where(x => App.Downloads.Shows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
    }

    /// <summary>
    /// Method gets most recent episode from each podcast on twit.tv
    /// </summary>
    /// <returns></returns>
    public async Task GetMostRecent()
    {
        if (App.MostRecentShows.Count > 0)
        {
            App.MostRecentShows.ForEach(MostRecentShows.Add);
            MostRecentShows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
            MostRecentShows?.Where(x => App.Downloads.Shows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
            return;
        }
        var temp = await UpdateMostRecentShows();
        var deDupe = RemoveDuplicates(temp);
        var item = deDupe.OrderBy(x => x.Title).ToList();
        item.ForEach(App.MostRecentShows.Add);
        item.ForEach(MostRecentShows.Add);
        MostRecentShows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        MostRecentShows?.Where(x => App.Downloads.Shows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        _logger.Info("Got Most recent shows");
    }
    public static async Task<List<Show>> UpdateMostRecentShows()
    {
        var shows = new List<Show>();
        var temp = await App.PositionData.GetAllPodcasts();
        temp?.Where(x => !x.Deleted).ToList().ForEach(show =>
        {
            var item = FeedService.GetShows(show.Url, true);
            if (item.Count > 0)
            {

                shows.Add(item[0]);
            }
        });
        return shows;
    }
    #endregion
}
