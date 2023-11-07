// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;

public partial class ShowService : BaseViewModel, IShowService
{
    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of <see cref="Show"/> managed by this class.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Show> _shows;

    /// <summary>
    /// An <see cref="ILogger"/> instance managed by this class.
    /// </summary>
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(ShowService));
    private readonly IFeedService _feedService;
    private readonly IDownloadShows _downloadShows;
    public ShowService(IConnectivity connectivity, IFeedService feedService, IDownloadShows downloadShows) : base(connectivity)
    {
        _feedService = feedService;
        _downloadShows = downloadShows;
        _shows = [];
        BindingBase.EnableCollectionSynchronization(Shows, null, ObservableCollectionCallback);
    }
    /// <summary>
    /// <c>GetShows</c> is a <see cref="Task"/> that takes a <see cref="string"/> for Url and returns a <see cref="Show"/>
    /// </summary>
    /// <param name="url"></param> <see cref="string"/> URL of Twit tv Show
    /// <param name="getFirstOnly"><see cref="bool"/> Get first item only.</param>
    /// <returns><see cref="Show"/></returns>
    public ObservableCollection<Show> GetShowsAsync(string url, bool getFirstOnly)
    {
        if (!InternetConnected())
        {
            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
            return new ObservableCollection<Show>();
        }

        Shows.Clear();
        var temp = _feedService.GetShows(url, getFirstOnly);
        temp.ForEach(SetProperties);
        Shows = new ObservableCollection<Show>(temp);
        _logger.Info("Got All Shows");
        return new ObservableCollection<Show>(temp);

    }
    public void SetProperties(Show show)
    {
        var downloads = _downloadShows.DownloadedShows.Any(x => x.Url == show.Url);
        if (downloads)
        {
            show.IsDownloaded = true;
            show.IsDownloading = false;
            show.IsNotDownloaded = false;
            return;
        }
        var currentDownload = Shows.ToList().Find(x => x.Url == show.Url);
        if (currentDownload is not null)
        {
            show.IsDownloaded = false;
            show.IsDownloading = true;
            show.IsNotDownloaded = false;
            return;
        }
        show.IsDownloading = false;
        show.IsNotDownloaded = true;
        show.IsDownloaded = false;
    }
    public void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
    {
        // `lock` ensures that only one thread access the collection at a time
        lock (collection)
        {
            accessMethod?.Invoke();
        }
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
        App.OnVideoNavigated.Add(show);
    }
}
