// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="DownloadedShowViewModel"/>
/// </summary>
public partial class DownloadedShowViewModel : BaseViewModel
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
    public async Task Tap(string url)
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

    /// <summary>
    /// Deletes file and removes it from database.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>

    [RelayCommand]
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
        _logger.LogInformation("Removed {file} from Downloaded Shows list.", url);
    }
}

