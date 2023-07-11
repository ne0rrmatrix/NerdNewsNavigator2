﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Shared;

[QueryProperty("Url", "Url")]
public partial class SharedViewModel : BaseViewModel
{
    #region Properties

    /// <summary>
    /// An <see cref="ILogger{TCategoryName}"/> instance managed by this class.
    /// </summary>
    private ILogger<BaseViewModel> Logger { get; set; }
    private string Item { get; set; }
    /// <summary>
    /// A private <see cref="string"/> that contains a Url for <see cref="Show"/>
    /// </summary>
    [ObservableProperty]
    private string _url;
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
        if (App.AllShows.Count == 0 && !App.Started)
        {
            Task.Run(GetMostRecent);
        }
#if WINDOWS || MACCATALYST || IOS
        if (DownloadService.IsDownloading)
        {
            ThreadPool.QueueUserWorkItem(state => { UpdatingDownload(); });
        }
#endif
    }

    #region Events
    partial void OnUrlChanged(string oldValue, string newValue)
    {
        var decodedUrl = HttpUtility.UrlDecode(newValue);
        Item = decodedUrl;
        GetShowsAsync(decodedUrl, false);
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
        WeakReferenceMessenger.Default.Send(new UrlItemMessage(item));
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}?Url={url}");

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
        await App.PositionData.DeleteDownload(item);
        DownloadedShows.Remove(item);
        Logger.LogInformation("Removed {file} from Downloaded Shows list.", url);
        var show = GetShowForDownload(url);
        if (show != null)
        {
            await Dnow.Update(show);
        }
        Logger.LogInformation("Failed to find a show to update");
        _ = Task.Run(async () =>
        {
            await App.GetMostRecent();
            await GetMostRecent();
        });
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
        _ = Task.Run(async () =>
        {
            await Downloading(url);
        });
#endif
#if ANDROID || IOS
        DownloadService.CancelDownload = false;
        var item = Shows.First(x => x.Url == url);
        await NotificationService.CheckNotification();
        var requests = await NotificationService.NotificationRequests(item);
        NotificationService.AfterNotifications(requests);
        _ = Task.Run(async () =>
        {
            await Downloading(url);
        });
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
    #endregion
}
