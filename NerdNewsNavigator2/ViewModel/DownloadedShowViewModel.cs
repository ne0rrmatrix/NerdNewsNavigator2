// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

public partial class DownloadedShowViewModel : BaseViewModel
{
    #region Properties

    private readonly ILogger<DownloadedShowViewModel> _logger;
    /// <summary>
    /// Intilializes an instance of <see cref="DownloadedShowViewModel"/>
    /// <paramref name="logger"/>
    /// </summary>

    #endregion

    public DownloadedShowViewModel(ILogger<DownloadedShowViewModel> logger)
        : base(logger)
    {
        _logger = logger;
        _logger.LogInformation("DownloadedShowViewModel started.");
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(DownloadedShows));
    }

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="TabletPlayPodcastPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Tap(string url)
    {
#if ANDROID || IOS
        var item = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), url);
        _logger.LogInformation("Url being passed is: {name}", item);
        await Shell.Current.GoToAsync($"{nameof(TabletPlayPodcastPage)}?Url={item}");
#endif
#if WINDOWS
        var item = "ms-appdata:///LocalCache/Local/" + url;
        _logger.LogInformation("Url being passed is: {name}", item);
        await Shell.Current.GoToAsync($"{nameof(TabletPlayPodcastPage)}?Url={item}");
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
        foreach (var item in DownloadedShows.ToList())
        {
            if (item.Url == url)
            {
                var filename = DownloadService.GetFileName(url);
                var tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                        _logger.LogInformation("Deleted file {file}", tempFile);
                    }
                    else
                    {
                        _logger.LogInformation("File {file} was not found in file system.", tempFile);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to delete file: {file} {Message}", tempFile, ex.Message);
                }
                await App.PositionData.DeleteDownload(item);
                DownloadedShows.Remove(item);
                _logger.LogInformation("Removed {file} from Downloaded Shows list.", url);
            }
        }
    }
}

