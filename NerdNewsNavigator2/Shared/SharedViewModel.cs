// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Input;
using NerdNewsNavigator2.Controls;

namespace NerdNewsNavigator2.Shared;

[QueryProperty("Url", "Url")]
public partial class SharedViewModel : BaseViewModel
{
    #region Properties
    PodcastServices PodcastServices { get; set; } = new();
    /// <summary>
    /// An <see cref="ILogger{TCategoryName}"/> instance managed by this class.
    /// </summary>
    private ILogger<BaseViewModel> Logger { get; set; }
    /// <summary>
    /// A private <see cref="string"/> that contains a Url for <see cref="Show"/>
    /// </summary>
    [ObservableProperty]
    private string _url;

    [ObservableProperty]
    private bool _isRefreshing;

    #endregion
    public SharedViewModel(ILogger<SharedViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        Logger = logger;
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        Orientation = OnDeviceOrientationChange();
        if (!InternetConnected())
        {
            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
        }
    }

    private void DownloadStarted(object sender, DownloadEventArgs e)
    {
        if (e.Status is null)
        {
            return;
        }
        Title = e.Status;
    }

    private void DownloadCompleted(object sender, DownloadEventArgs e)
    {
        if (e.Item is null)
        {
            return;
        }
        App.Downloads.DownloadFinished -= DownloadCompleted;
        Debug.WriteLine("Downloaded event firing");
        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            IsBusy = false;
            Title = string.Empty;
            DownloadProgress = string.Empty;
            if (Shows.ToList().Exists(x => x.Url == e.Item.Url))
            {
                var number = Shows.IndexOf(e.Item);
                Shows[number].IsDownloaded = true;
                Shows[number].IsDownloading = false;
                Shows[number].IsNotDownloaded = false;
                OnPropertyChanged(nameof(Shows));
            }
            if (MostRecentShows.ToList().Exists(x => x.Url == e.Item.Url))
            {
                var number = MostRecentShows.IndexOf(e.Item);
                this.MostRecentShows[number].IsDownloaded = true;
                this.MostRecentShows[number].IsDownloading = false;
                this.MostRecentShows[number].IsNotDownloaded = false;
                OnPropertyChanged(nameof(MostRecentShows));
            }
        });
    }
    #region Commands
    public ICommand PullToRefreshCommand => new Command(() =>
    {
        Debug.WriteLine("Starting refresh");
        IsRefreshing = true;
        RefreshData();
        IsRefreshing = false;
        Debug.WriteLine("Refresh done");
    });
    public Task RefreshData()
    {
        IsBusy = true;
        Shows.Clear();
        MostRecentShows.Clear();
        Podcasts.Clear();
        ThreadPool.QueueUserWorkItem(state => GetMostRecent());
        ThreadPool.QueueUserWorkItem(async state => await GetUpdatedPodcasts());
        GetShowsAsync(Url, false);
        IsBusy = false;
        return Task.CompletedTask;
    }
    #endregion

    #region Events
    partial void OnUrlChanged(string oldValue, string newValue)
    {
        Debug.WriteLine("Url Changed");
        var decodedUrl = HttpUtility.UrlDecode(newValue);
#if WINDOWS || MACCATALYST || ANDROID
        ThreadPool.QueueUserWorkItem((state) => GetShowsAsync(decodedUrl, false));
#endif
#if IOS
        GetShowsAsync(decodedUrl, false);
#endif
    }

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="PodcastPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Tap(string url)
    {
        var item = Shows.FirstOrDefault(x => x.Url == url) ?? MostRecentShows.FirstOrDefault(x => x.Url == url);
        if (DownloadedShows.ToList().Exists(x => x.Url == item.Url))
        {
#if ANDROID || IOS || MACCATALYST
            var download = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DownloadService.GetFileName(url));
            await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}?Url={download}");
#endif
#if WINDOWS
            var download = "ms-appdata:///LocalCache/Local/" + DownloadService.GetFileName(url);
            await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}?Url={download}");
#endif
            return;
        }
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}?Url={url}");
        WeakReferenceMessenger.Default.Send(new UrlItemMessage(item));
    }

    #endregion

    #region Shared ViewModel code

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
        Logger.LogInformation("Removed {file} from Downloaded Shows list.", url);
        Logger.LogInformation("Failed to find a show to update");
        MostRecentShows.Clear();
        ThreadPool.QueueUserWorkItem(async state => await App.GetMostRecent());
        GetMostRecent();
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
        item.IsDownloaded = false;
        item.IsDownloading = true;
        item.IsNotDownloaded = false;
        if (Shows.ToList().Exists(x => x.Url == item.Url))
        {
            Shows[Shows.IndexOf(item)] = item;
        }
        if (MostRecentShows.ToList().Exists(x => x.Url == item.Url))
        {
            MostRecentShows[MostRecentShows.IndexOf(item)] = item;
        }
        App.Downloads.DownloadStarted += DownloadStarted;
        App.Downloads.DownloadFinished += DownloadCompleted;
        App.Downloads.Add(item);
        App.Downloads.Start(item);
    }

    #endregion
    #region Download Status Methods

    public void SetProperties(Show show)
    {
        var item = GetShowForDownload(show.Url);
        if (item is null)
        {
            return;
        }
        var shows = Shows.ToList().Find(x => x.Url == item.Url);
        var recent = MostRecentShows.ToList().Find(x => x.Url == item.Url);
        var currentDownload = App.Downloads.Shows.Find(x => x.Url == item.Url);
        var downloads = DownloadedShows.ToList().Find(x => x.Url == item.Url);

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
        if (Shows.ToList().Exists(x => x.Url == show.Url))
        {
            var number = Shows.IndexOf(show);
            Shows[number].IsDownloaded = show.IsDownloaded;
            Shows[number].IsDownloading = show.IsDownloading;
            Shows[number].IsNotDownloaded = show.IsNotDownloaded;
            OnPropertyChanged(nameof(Shows));
        }
        if (MostRecentShows.ToList().Exists(x => x.Url == show.Url))
        {
            var number = MostRecentShows.IndexOf(show);
            MostRecentShows[number].IsDownloaded = show.IsDownloaded;
            MostRecentShows[number].IsDownloading = show.IsDownloading;
            MostRecentShows[number].IsNotDownloaded = show.IsNotDownloaded;
            OnPropertyChanged(nameof(MostRecentShows));
        }
    }
    public void SetCancelData(Show item)
    {
        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            IsBusy = false;
            Title = string.Empty;
            DownloadProgress = string.Empty;
            if (Shows.ToList().Exists(x => x.Url == item.Url))
            {
                var number = Shows.IndexOf(item);
                Shows[number].IsDownloaded = false;
                Shows[number].IsDownloading = false;
                Shows[number].IsNotDownloaded = true;
                OnPropertyChanged(nameof(Shows));
            }
            if (MostRecentShows.ToList().Exists(x => x.Url == item.Url))
            {
                var number = MostRecentShows.IndexOf(item);
                MostRecentShows[number].IsDownloaded = false;
                MostRecentShows[number].IsDownloading = false;
                this.MostRecentShows[number].IsNotDownloaded = true;
                OnPropertyChanged(nameof(MostRecentShows));
            }
        });
    }
    #endregion
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
        item.ForEach(async x => await PodcastServices.UpdateImage(x));
        Shows.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        Shows.Where(x => App.Downloads.Shows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        Logger.LogInformation("Got All Shows");
    }

    /// <summary>
    /// Method gets most recent episode from each podcast on twit.tv
    /// </summary>
    /// <returns></returns>
    public void GetMostRecent()
    {
        while (App.Loading)
        {
            Thread.Sleep(500);
        }
        var deDupe = RemoveDuplicates(App.MostRecentShows);
        var item = deDupe.OrderBy(x => x.Title).ToList();
        item.ForEach(MostRecentShows.Add);
        item.ForEach(async x => await PodcastServices.UpdateImage(x));
        MostRecentShows.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        MostRecentShows.Where(x => App.Downloads.Shows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        Logger.LogInformation("Got Most recent shows");
    }

}
