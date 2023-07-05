// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Messaging;

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="ShowViewModel"/>
/// </summary>
[QueryProperty("Url", "Url")]
public partial class ShowViewModel : BaseViewModel
{
    #region Properties

    /// <summary>
    /// An <see cref="ILogger{TCategoryName}"/> instance managed by this class.
    /// </summary>
    private readonly ILogger<ShowViewModel> _logger;

    private string Item { get; set; }
    /// <summary>
    /// A private <see cref="string"/> that contains a Url for <see cref="Show"/>
    /// </summary>
    [ObservableProperty]
    private string _url;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ShowViewModel"/> class.
    /// </summary>
    public ShowViewModel(ILogger<ShowViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        _logger = logger;
        WeakReferenceMessenger.Default.Register<DownloadStatusMessage>(this, (r, m) =>
        {
            RecievedDownloadSttusMessage(m.Value, m.Now);
        });

        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        Orientation = OnDeviceOrientationChange();
        if (!InternetConnected())
        {
            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
        }
#if WINDOWS || MACCATALYST || IOS
        if (DownloadService.IsDownloading)
        {
            ThreadPool.QueueUserWorkItem(state => { UpdatingDownload(); });
        }
#endif
    }
    public void RecievedDownloadSttusMessage(bool value, Show item)
    {
        if (item is not null)
        {
            item.IsDownloading = value;
            _ = MainThread.InvokeOnMainThreadAsync(() => Dnow.Update(item));
        }
    }
    #region Events
    partial void OnUrlChanged(string oldValue, string newValue)
    {
        var decodedUrl = HttpUtility.UrlDecode(newValue);
        Item = decodedUrl;
        GetShows(decodedUrl, false);
    }
    public async Task Delete(string url)
    {
        var item = DownloadedShows.First(x => x.Url == url);
        if (item is null)
        {
            return;
        }
        var filename = DownloadService.GetFileName(item.Url);
        var tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
        if (File.Exists(tempFile))
        {
            File.Delete(tempFile);
            _logger.LogInformation("Deleted file {file}", tempFile);
            WeakReferenceMessenger.Default.Send(new DeletedItemMessage(true));
        }
        else
        {
            _logger.LogInformation("File {file} was not found in file system.", tempFile);
        }
        item.IsDownloaded = false;
        item.Deleted = true;
        item.IsNotDownloaded = true;
        await App.PositionData.UpdateDownload(item);
        DownloadedShows.Remove(item);
        SetDataAsync(url);
        _logger.LogInformation("Removed {file} from Downloaded Shows list.", url);
    }
    private void SetDataAsync(string url)
    {
        var allShow = App.AllShows.First(x => x.Url == url);
        allShow.IsDownloaded = false;
        allShow.IsNotDownloaded = true;
        allShow.IsDownloading = false;
        App.AllShows[App.AllShows.IndexOf(allShow)] = allShow;
        Dnow.Update(allShow);
    }

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="PodcastPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Tap(string url)
    {
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}?Url={url}");
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
        RunDownloads(url);
#endif
#if ANDROID || IOS
        DownloadService.CancelDownload = false;
        var item = Shows.First(x => x.Url == url);
        await NotificationService.CheckNotification();
        var requests = await NotificationService.NotificationRequests(item);
        NotificationService.AfterNotifications(requests);
        RunDownloads(url);
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
        var itemUrl = Shows.ToList().Find(x => x.Url == url);
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
