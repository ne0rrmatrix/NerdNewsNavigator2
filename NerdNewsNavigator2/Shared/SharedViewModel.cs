﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Input;

namespace NerdNewsNavigator2.Shared;

[QueryProperty("Url", "Url")]
public partial class SharedViewModel : BaseViewModel
{
    #region Properties
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
        App.CurrentNavigation.NavigationCompleted += OnNavigated;
        App.Downloads.DownloadStarted += DownloadStarted;
        if (!InternetConnected())
        {
            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
        }
    }
    private void OnNavigated(object sender, Primitives.NavigationEventArgs e)
    {
        if (e.IsNavigating)
        {
            App.CurrentNavigation.IsNavigating = false;
            Debug.WriteLine("OnNavigated event Firing");
            ThreadPool.QueueUserWorkItem(state =>
            {
                switch (e.IsShows)
                {
                    case true:
                        while (Shows.Count == 0)
                        {
                            Thread.Sleep(100);
                        }
                        Shows.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
                        Shows.Where(x => App.Downloads.Shows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
                        break;

                    case false:
                        while (MostRecentShows.Count == 0)
                        {
                            Thread.Sleep(100);
                        }
                        MostRecentShows.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
                        MostRecentShows.Where(x => App.Downloads.Shows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
                        break;
                }
            });
            App.CurrentNavigation.NavigationCompleted -= OnNavigated;
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
    public async void DownloadCompleted(object sender, DownloadEventArgs e)
    {
        if (App.Downloads.Shows.Count == 0)
        {
            Title = string.Empty;
            App.Downloads.DownloadFinished -= DownloadCompleted;
        }
        await GetDownloadedShows();
        Debug.WriteLine("Shared View model - Downloaded event firing");
        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            IsBusy = false;
            Title = string.Empty;
            DownloadProgress = string.Empty;

            MostRecentShows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(item =>
            {
                var number = MostRecentShows.IndexOf(item);
                MostRecentShows[number].IsDownloaded = false;
                MostRecentShows[number].IsDownloading = false;
                MostRecentShows[number].IsNotDownloaded = true;
                OnPropertyChanged(nameof(MostRecentShows));
            });
            Shows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(item =>
            {
                var number = Shows.IndexOf(item);
                Shows[number].IsDownloaded = false;
                Shows[number].IsDownloading = false;
                Shows[number].IsNotDownloaded = true;
                OnPropertyChanged(nameof(Shows));
            });
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
        ThreadPool.QueueUserWorkItem(async state => await GetUpdatedPodcasts());
        ThreadPool.QueueUserWorkItem(async state => await GetMostRecent());
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
        if (DownloadedShows.Where(y => !y.Deleted).ToList().Exists(x => x.Url == url))
        {
            var download = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DownloadService.GetFileName(url));
            await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}");
            var item = DownloadedShows.ToList().Find(x => x.Url == url);
            Show show = new()
            {
                Url = download,
                Title = item.Title,
            };
#if ANDROID || IOS || MACCATALYST
            App.OnVideoNavigated.Add(show);
#endif
#if WINDOWS
            await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}");
            App.OnVideoNavigated.Add(show);
#endif
            return;
        }
        var temp = Shows.ToList().Find(x => x.Url == url) ?? MostRecentShows.ToList().Find(y => y.Url == url) ?? throw new NullReferenceException();
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}");
        App.OnVideoNavigated.Add(temp);
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
            this.MostRecentShows[number].IsDownloaded = false;
            this.MostRecentShows[number].IsDownloading = true;
            this.MostRecentShows[number].IsNotDownloaded = false;
            OnPropertyChanged(nameof(MostRecentShows));
        }
        App.Downloads.DownloadFinished += DownloadCompleted;
        App.Downloads.Add(item);
        App.Downloads.Start(item);
    }

    #endregion
    #region Download Status Methods
    public void SetProperties(Show show)
    {
        var shows = Shows.FirstOrDefault(x => x.Url == show.Url);
        var recent = MostRecentShows.FirstOrDefault(x => x.Url == show.Url);
        var currentDownload = App.Downloads.Shows.Find(x => x.Url == show.Url);
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
    }

    public void SetCancelData(string url, bool isShow)
    {
        var item = App.Downloads.Cancel(url);
        if (item is null)
        {
            Logger.LogInformation("show was null");
            return;
        }
        Debug.WriteLine(item.Url);
        IsBusy = false;
        Title = string.Empty;
        DownloadProgress = string.Empty;
        if (isShow)
        {
            var exists = Shows.ToList().Exists(x => x.Url == item.Url);
            item = Shows.ToList().Find(x => x.Url == item.Url);
            if (exists)
            {
                var number = Shows.IndexOf(item);
                Shows[number].IsDownloaded = false;
                Shows[number].IsDownloading = false;
                Shows[number].IsNotDownloaded = true;
                OnPropertyChanged(nameof(Shows));
            }
        }
        else
        {
            var recent = MostRecentShows.ToList().Exists(x => x.Url == item.Url);
            item = MostRecentShows.ToList().Find(x => x.Url == item.Url);
            if (recent)
            {
                var number = MostRecentShows.IndexOf(item);
                MostRecentShows[number].IsDownloaded = false;
                MostRecentShows[number].IsDownloading = false;
                this.MostRecentShows[number].IsNotDownloaded = true;
                OnPropertyChanged(nameof(MostRecentShows));
            }
        }
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
        Logger.LogInformation("Got All Shows");
    }

    /// <summary>
    /// Method gets most recent episode from each podcast on twit.tv
    /// </summary>
    /// <returns></returns>
    public async Task GetMostRecent()
    {
        if (MostRecentShows.Count > 0)
        {
            await CheckForNew();
        }
        var deDupe = RemoveDuplicates(App.MostRecentShows);
        var item = deDupe.OrderBy(x => x.Title).ToList();
        item.ForEach(MostRecentShows.Add);
        Logger.LogInformation("Got Most recent shows");
    }
    public async Task CheckForNew()
    {
        if (App.Loading || App.MostRecentShows.Count > 0)
        {
            Logger.LogInformation("App loading. Returning");
            return;
        }
        Logger.LogInformation("Checking for new Most Recent Shows");
        await App.GetMostRecent();
        App.MostRecentShows.Where(x => !(MostRecentShows.ToList().Exists(y => y.Url == x.Url))).ToList().ForEach(MostRecentShows.Add);
    }
}
