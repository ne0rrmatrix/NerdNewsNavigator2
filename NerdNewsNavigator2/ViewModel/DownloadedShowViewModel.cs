// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="DownloadedShowViewModel"/>
/// </summary>
public partial class DownloadedShowViewModel : SharedViewModel
{
    #region Properties
    /// <summary>
    /// An <see cref="ILogger{TCategoryName}"/> instance managed by this class.
    /// </summary>
    private readonly ILogger<DownloadedShowViewModel> _logger;

    #endregion
    /// <summary>
    /// Intilializes an instance of <see cref="DownloadedShowViewModel"/>
    /// <paramref name="logger"/>
    /// </summary>
    public DownloadedShowViewModel(ILogger<DownloadedShowViewModel> logger, IConnectivity connectivity)
        : base(logger, connectivity)
    {
        _logger = logger;
        _logger.LogInformation("DownloadedShowViewModel started.");
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        Orientation = OnDeviceOrientationChange();
        if (!InternetConnected())
        {
            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
        }
        Task.Run(async () =>
        {
            await GetDownloadedShows();
        });
#if WINDOWS || MACCATALYST || IOS
        if (DownloadService.IsDownloading)
        {
            ThreadPool.QueueUserWorkItem(state => { UpdatingDownload(); });
        }
#endif
    }

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="VideoPlayerPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task PlayDownloadedShow(string url)
    {
#if ANDROID || IOS || MACCATALYST
        var item = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), url);
        _logger.LogInformation("Url for file is: {name}", item);
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}?Url={item}");
#endif
#if WINDOWS
        var item = "ms-appdata:///LocalCache/Local/" + url;
        _logger.LogInformation("Url being passed is: {name}", item);
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}?Url={item}");
#endif
    }
}

