// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that manages the 
/// </summary>
public partial class MostRecentShowsViewModel : BaseViewModel
{
    private readonly ILogger<MostRecentShowsViewModel> _logger;
    /// <summary>
    /// Initializes a new instance of <see cref="MostRecentShowsViewModel"/>
    /// <paramref name="logger"/>
    /// </summary>
    public MostRecentShowsViewModel(ILogger<MostRecentShowsViewModel> logger)
    : base(logger)
    {
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
        _logger = logger;
    }
    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="MostRecentShowPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    async Task Download(string url)
    {
        _logger.LogInformation("Trying to start download of {URL}", url);
        IsBusy = true;
        foreach (var item in MostRecentShows.ToList())
        {
            if (item.Url == url)
            {
                _logger.LogInformation("Found match!");
                Download download = new()
                {
                    Title = item.Title,
                    Url = url,
                    Image = item.Image,
                    PubDate = item.PubDate,
                    Description = item.Description,
                    FileName = DownloadService.GetFileName(url)
                };
                var downloaded = await DownloadService.DownloadShow(download);
                if (downloaded)
                {
                    _logger.LogInformation("Downloaded file: {file}", download.FileName);
                    var result = await App.PositionData.GetAllDownloads();
                    foreach (var show in result)
                    {
                        if (show.Title == download.Title)
                        {
                            await App.PositionData.DeleteDownload(show);
                        }
                    }

                    await DownloadService.AddDownloadDatabase(download);
                    IsBusy = false;
                }
                else
                {
                    IsBusy = false;

                }
                return;
            }
        }
    }

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="MostRecentShowsPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]

    async Task Tap(string url) => await Shell.Current.GoToAsync($"{nameof(TabletPlayPodcastPage)}?Url={url}");
}
