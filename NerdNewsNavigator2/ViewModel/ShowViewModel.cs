// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="ShowViewModel"/>
/// </summary>
[QueryProperty("Url", "Url")]
public partial class ShowViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<Show> _shows;

    [ObservableProperty]
    private string _url;

    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(ShowViewModel));
    private readonly IShowService _showService;
    private readonly IDownloadService _downloadService;
    private readonly IDownloadShows _downloadShows;
    private readonly IVideoOnNavigated _videoOnNavigated;
    private readonly ICurrentDownloads _currentDownloads;
    private readonly IDeletedItemService _deletedItemService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShowViewModel"/> class.
    /// </summary>
    /// <param name="connectivity"></param>
    /// <param name="downloadShows"></param>
    /// <param name="showService"></param>
    /// <param name="downloadService"></param>
    /// <param name="videoOnNavigated"></param>
    /// <param name="currentDownloads"></param>
    public ShowViewModel(IConnectivity connectivity, IDownloadShows downloadShows, IShowService showService, IDownloadService downloadService, IVideoOnNavigated videoOnNavigated, ICurrentDownloads currentDownloads, IDeletedItemService deletedItemService) : base(connectivity)
    {
        _showService = showService;
        _downloadService = downloadService;
        _downloadShows = downloadShows;
        _videoOnNavigated = videoOnNavigated;
        _currentDownloads = currentDownloads;
        _deletedItemService = deletedItemService;
        _currentDownloads.DownloadStarted += DownloadStarted;
        _currentDownloads.DownloadFinished += ShowsDownloadCompleted;
        _currentDownloads.DownloadCancelled += DownloadCancelled;
        _deletedItemService.DeletedItem += OnItemDeleted;
    }
    partial void OnUrlChanged(string oldValue, string newValue)
    {
        _logger.Info("Show Url changed. Updating Shows");
        if (!InternetConnected())
        {
            return;
        }
        var decodedUrl = HttpUtility.UrlDecode(newValue);
        ThreadPool.QueueUserWorkItem(state => Shows = _showService.GetShowsAsync(decodedUrl, false));
    }
    private async void OnItemDeleted(object sender, DeletedItemEventArgs e)
    {
        await _downloadShows.GetDownloadedShows();
        _logger.Info("Updating deleted Items");
        Shows?.Where(x => x.Url == e.Item.Url).ToList().ForEach(_showService.SetProperties);
    }
    public ICommand PullToRefreshCommand => new Command(async () =>
    {
        _logger.Info("Starting Show refresh");
        IsRefreshing = true;
        await RefreshData();
        IsRefreshing = false;
        _logger.Info("Show Refresh is done");
    });
    public async Task RefreshData()
    {
        IsBusy = true;
        Shows.Clear();
        _downloadShows.DownloadedShows.Clear();
        await _downloadShows.GetDownloadedShows();
        Shows = _showService.GetShowsAsync(Url, false);
        IsBusy = false;
    }
    private async void ShowsDownloadCompleted(object sender, DownloadEventArgs e)
    {
        _ = MainThread.InvokeOnMainThreadAsync(() => Title = e.Title);
        await _downloadShows.GetDownloadedShows();
        Shows.Where(x => x.Title == e.Item.Title).ToList().ForEach(_showService.SetProperties);
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
        if (_downloadShows.DownloadedShows.Where(y => y.IsDownloaded).Any(x => x.Url == url))
        {
            var item = _downloadShows.DownloadedShows.ToList().Find(x => x.Url == url);
            show.Title = item.Title;
            show.Url = item.FileName;
        }
        else
        {
            var item = Shows.First(x => x.Url == url);
            show.Url = item.Url;
            show.Title = item.Title;
        }
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}");
        _videoOnNavigated.Add(show);
    }
    [RelayCommand]
#pragma warning disable CA1822 // Mark members as static will break functionality. Class data is being modified and xaml will not update with static modifier.
    public void Cancel(Show show)
#pragma warning restore CA1822 // Mark members as static will break functionality. Class data is being modified and xaml will not update with static modifier.
    {
        show.IsDownloading = false;
        show.IsNotDownloaded = true;
        show.IsDownloaded = false;
        _downloadService.Cancel(show);
    }
    /// <summary>
    /// A Method that passes a Url to <see cref="DownloadService"/>
    /// </summary>
    /// <param name="show">A Url <see cref="Show"/></param>
    /// <returns></returns>
    [RelayCommand]
    public void Download(Show show)
    {
#if ANDROID
        _ = EditViewModel.CheckAndRequestForeGroundPermission();
#endif
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Toast.Make("Added show to downloads.", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
        });
        show.IsDownloaded = false;
        show.IsDownloading = true;
        show.IsNotDownloaded = false;
        _downloadService.Add(show);
        ThreadPool.QueueUserWorkItem(state => _ = _downloadService.Start(show));
    }
}
