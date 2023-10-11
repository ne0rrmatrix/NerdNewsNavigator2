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

    #region Events
    partial void OnUrlChanged(string oldValue, string newValue)
    {
        _logger.Info("Show Url changed. Updating Shows");
        if (!InternetConnected())
        {
            return;
        }
        var decodedUrl = HttpUtility.UrlDecode(newValue);
#if WINDOWS || MACCATALYST || ANDROID
        ThreadPool.QueueUserWorkItem(state => GetShowsAsync(decodedUrl, false));
#endif
#if IOS
        GetShowsAsync(decodedUrl, false);
#endif
    }
    public void OnNavigated(object sender, Primitives.NavigationEventArgs e)
    {
        Shows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        Shows?.Where(x => App.Downloads.Shows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
    }
    public void DownloadStarted(object sender, DownloadEventArgs e)
    {
        if (e.Status is null || e.Shows.Count == 0)
        {
            return;
        }
        Title = e.Status;
    }
    public void DonwnloadCancelled(object sender, DownloadEventArgs e)
    {
        App.Downloads.DownloadCancelled -= DonwnloadCancelled;
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            Title = string.Empty;
            OnPropertyChanged(nameof(Title));
        });
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
            _logger.Info("update for shows not downlaoding anymore");
            Title = string.Empty;
            OnPropertyChanged(nameof(Title));
        });
    }
    public async void DownloadCompleted(object sender, DownloadEventArgs e)
    {
        Title = string.Empty;
        OnPropertyChanged(nameof(Title));
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
        Show show = new();
        if (DownloadedShows.Where(y => y.IsDownloaded).ToList().Exists(x => x.Url == url))
        {
            var item = DownloadedShows.ToList().Find(x => x.Url == url);
            show.Url = item.Url;
            show.Title = item.Title;
        }
        else
        {
            var item = Shows.ToList().Find(x => x.Url == url);
            show.Url = item.Url;
            show.Title = item.Title;
        }
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}");
        App.OnVideoNavigated.Add(show);
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
        var showTemp = Shows.ToList().Find(x => x.Url == url);
        Shows?.Remove(showTemp);
        _logger.Info($"Removed {url} from Downloaded Shows list.");
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
        OnPropertyChanged(nameof(Title));
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
        DownloadProgress = string.Empty;
    }
    #endregion

    #region Download Status Methods
    public void SetProperties(Show show)
    {
        var shows = Shows.FirstOrDefault(x => x.Url == show.Url);
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
        if (!InternetConnected())
        {
            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
            return;
        }
        Shows.Clear();
        var temp = FeedService.GetShows(url, getFirstOnly);
        var item = RemoveDuplicates(temp);
        item.ForEach(Shows.Add);
        _logger.Info("Got All Shows");
        Shows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        Shows?.Where(x => App.Downloads.Shows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
    }
    public void UpdateShows()
    {
        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            IsBusy = false;
            Title = string.Empty;
            DownloadProgress = string.Empty;
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
    #endregion
}
