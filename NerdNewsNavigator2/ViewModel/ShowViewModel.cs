// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Plugin.LocalNotification;
using Plugin.LocalNotification.iOSOption;

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="ShowViewModel"/>
/// </summary>
[QueryProperty("Url", "Url")]
public partial class ShowViewModel : BaseViewModel
{
    #region Properties
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
    partial void OnUrlChanged(string oldValue, string newValue)
    {
        var decodedUrl = HttpUtility.UrlDecode(newValue);
        GetShows(decodedUrl, false);
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
    /// A Method that passes a Url <see cref="string"/> to <see cref="ShowPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Download(string url)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Toast.Make("Added show to downloads.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        });
        var item = Shows.First(x => x.Url == url);
        if (item != null)
        {
            item.IsDownloaded = true;
            item.IsNotDownloaded = false;
            Shows[Shows.IndexOf(item)] = item;
        }

#if WINDOWS || MACCATALYST
        await Downloading(url, false);
#endif
#if ANDROID || IOS
        await UpdateNotification(item, url);
#endif
    }

#if ANDROID || IOS
    public async Task UpdateNotification(Show item, string url)
    {
        await LocalNotificationCenter.Current.RequestNotificationPermission();
        var request = new Plugin.LocalNotification.NotificationRequest
        {
            NotificationId = 1337,
            Title = item?.Title,
            Description = $"Download Progress {(int)ProgressInfos}",
#if ANDROID
            Android = new AndroidOptions
            {
                IconSmallName = new AndroidIcon("ic_stat_alarm"),
                ProgressBarProgress = (int)ProgressInfos,
                IsProgressBarIndeterminate = false,
                Color =
                    {
                        ResourceName = "colorPrimary"
                    },
                AutoCancel = true,
                ProgressBarMax = 100,
            },
#endif
        };
        await LocalNotificationCenter.Current.Show(request);

        _ = Task.Run(async () =>
        {
            await Downloading(url, false);
        });

        while (DownloadService.IsDownloading)
        {
            Thread.Sleep(1000);
            request.Description = $"Download Progress {(int)ProgressInfos}%";
            request.Android.ProgressBarProgress = (int)ProgressInfos;
            await LocalNotificationCenter.Current.Show(request);
        }
        request.Android.ProgressBarProgress = 100;
        request.Description = "Download Complete";
        await LocalNotificationCenter.Current.Show(request);
    }
#endif

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="VideoPlayerPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Play(string url)
    {
#if ANDROID || IOS || MACCATALYST
        var item = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DownloadService.GetFileName(url));
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}?Url={item}");
#endif
#if WINDOWS
        var item = "ms-appdata:///LocalCache/Local/" + DownloadService.GetFileName(url);
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}?Url={item}");
#endif
    }
}
