// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="DownloadedShowViewModel"/>
/// </summary>
public partial class DownloadedShowViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<Download> _downloadedShows;
    /// <summary>
    /// Initializes a new instance of the <see cref="ILogger"/> class
    /// </summary>
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(DownloadedShowViewModel));

    private readonly IFileService _fileService;
    private readonly IShowService _showService;
    private readonly IDownloadShows _downloadShowService;

    /// <summary>
    /// Initializes an instance of <see cref="DownloadedShowViewModel"/>
    /// <paramref name="connectivity"/>
    /// </summary>
    public DownloadedShowViewModel(IConnectivity connectivity, IShowService showService, IFileService fileService, IDownloadShows downloadService)
        : base(connectivity)
    {
        _fileService = fileService;
        _showService = showService;
        _downloadShowService = downloadService;
        _ = GetDownloadedShowsAsync();
        App.Downloads.DownloadStarted += DownloadStarted;
        App.Downloads.DownloadCancelled += DownloadCancelled;
        App.Downloads.DownloadFinished += Finished;
    }

    private async Task GetDownloadedShowsAsync()
    {
        DownloadedShows = new ObservableCollection<Download>(await _downloadShowService.GetDownloadedShows());
    }
    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="PodcastPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Play(string url)
    {
        Show show = new();
        if (DownloadedShows.Where(y => y.IsDownloaded).Any(x => x.Url == url))
        {
            var item = DownloadedShows.ToList().Find(x => x.Url == url);
            show.Title = item.Title;
            show.Url = item.FileName;
        }
        else
        {
            var item = _showService.Shows.First(x => x.Url == url);
            show.Url = item.Url;
            show.Title = item.Title;
        }
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}");
        App.OnVideoNavigated.Add(show);
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
        _downloadShowService.DownloadedShows.Clear();
        await _downloadShowService.GetDownloadedShows();
        IsBusy = false;
    }
    private void Finished(object sender, DownloadEventArgs e)
    {
        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            Title = e.Title;
            Download download = new()
            {
                Title = e.Item.Title,
                Url = e.Item.Url,
                Image = e.Item.Image,
                IsDownloaded = true,
                IsNotDownloaded = false,
                Deleted = false,
                PubDate = e.Item.PubDate,
                Description = e.Item.Description,
                FileName = _fileService.GetFileName(e.Item.Url)
            };
            _downloadShowService.DownloadedShows.Add(download);
            DownloadedShows.Add(download);
            _showService.Shows.Where(x => x.Title == e.Item.Title).ToList().ForEach(_showService.SetProperties);
        });
    }

    /// <summary>
    /// Method Deletes a <see cref="Download"/>
    /// </summary>
    /// <param name="download"><see cref="Download"/> to be Deleted.</param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Delete(Download download)
    {
        var tempFile = _fileService.GetFileName(download.Url);
        _fileService.DeleteFile(tempFile);
        download.IsDownloaded = false;
        download.Deleted = true;
        download.IsNotDownloaded = true;
        await App.PositionData.UpdateDownload(download);
        _downloadShowService.DownloadedShows.Remove(download);
        var showTemp = _showService.Shows.ToList().Find(x => x.Url == download.Url);
        _showService.Shows?.Remove(showTemp);
        _logger.Info($"Removed {download.FileName} from Downloaded Shows list.");
        App.DeletedItem.Add(download);
    }
}
