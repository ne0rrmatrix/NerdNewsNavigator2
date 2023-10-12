// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="DownloadedShowViewModel"/>
/// </summary>
public partial class DownloadedShowViewModel : BaseViewModel
{

    /// <summary>
    /// Initilizes a new instance of the <see cref="ILogger"/> class
    /// </summary>
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(DownloadedShowViewModel));

    /// <summary>
    /// Intilializes an instance of <see cref="DownloadedShowViewModel"/>
    /// <paramref name="connectivity"/>
    /// </summary>
    public DownloadedShowViewModel(IConnectivity connectivity)
        : base(connectivity)
    {
        App.Downloads.DownloadStarted += DownloadStarted;
        App.Downloads.DownloadCancelled += DownloadCancelled;
        App.Downloads.DownloadFinished += ShowsDownloadCompleted;
    }
    public ICommand PullToRefreshCommand => new Command(async () =>
    {
        _logger.Info("Starting refresh of Downloaded shows");
        IsRefreshing = true;
        await RefreshData();
        IsRefreshing = false;
        _logger.Info("Finished refreshing of Downloaded shows");
    });
    public async Task RefreshData()
    {
        IsBusy = true;
        DownloadedShows.Clear();
        await GetDownloadedShows();
        IsBusy = false;
    }
    private async void ShowsDownloadCompleted(object sender, DownloadEventArgs e)
    {
        await GetDownloadedShows();
        _ = MainThread.InvokeOnMainThreadAsync(() => { Title = string.Empty; });
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
        App.DeletedItem.Add(item);
    }
}

