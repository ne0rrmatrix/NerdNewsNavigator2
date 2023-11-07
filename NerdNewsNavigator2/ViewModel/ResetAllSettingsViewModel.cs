// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;
/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="ResetAllSettingsViewModel"/>
/// </summary>
public partial class ResetAllSettingsViewModel : BaseViewModel
{
    private readonly IMessenger _messenger;
    /// <summary>
    /// An <see cref="ILogger"/> instance managed by this class.
    /// </summary>
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(ResetAllSettingsViewModel));
    private readonly IPodcastService _podcastService;
    private readonly IShowService _showService;
    private readonly IDownloadService _downloadService;
    private readonly IDownloadShows _downloadShows;
    /// <summary>
    /// Initializes a new instance of the <see cref="ResetAllSettingsViewModel"/> class.
    /// </summary>
    public ResetAllSettingsViewModel(IConnectivity connectivity, IDownloadShows downloadShows, IPodcastService podcastService, IShowService showService, IMessenger messenger, IDownloadService downloadService) : base(connectivity)
    {
        Shell.Current.FlyoutIsPresented = false;
        _downloadShows = downloadShows;
        _downloadService = downloadService;
        _messenger = messenger;
        _podcastService = podcastService;
        _showService = showService;
        ThreadPool.QueueUserWorkItem(async state => await ResetAll());
    }

    #region Methods
    /// <summary>
    /// A Method to delete the <see cref="List{T}"/> of <see cref="Podcast"/>
    /// Function has to be public to work. I don't know why!
    /// </summary>
    private async Task ResetAll()
    {
        _messenger.Send(new MessageData(false));
        _downloadService.CancelAll();
        var item = await App.PositionData.GetAllDownloads();
        item.ForEach(App.DeletedItem.Add);
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        DeleteFiles(System.IO.Directory.GetFiles(path, "*.mp4"));
        await DeleteAllAsync();
        SetVariables();
        Thread.Sleep(500);
        await _podcastService.GetPodcasts();
        await _downloadShows.GetDownloadedShows();
        await GetFavoriteShows();
        Thread.Sleep(1000);
        await MainThread.InvokeOnMainThreadAsync(() => { Shell.Current.GoToAsync($"{nameof(SettingsPage)}"); });
    }
    private void SetVariables()
    {
        Preferences.Default.Clear();
        Title = string.Empty;
        FavoriteShows.Clear();
        _showService.Shows.Clear();
        _podcastService.Podcasts.Clear();
        _downloadShows.DownloadedShows.Clear();
    }
    private static async Task DeleteAllAsync()
    {
        await App.PositionData.DeleteAllPositions();
        await App.PositionData.DeleteAllPodcasts();
        await App.PositionData.DeleteAllDownloads();
        await App.PositionData.DeleteAllFavorites();
    }
    private void DeleteFiles(string[] files)
    {
        try
        {
            foreach (var file in files)
            {
                System.IO.File.Delete(file);
                _logger.Info($"Deleted file {file}");
            }
        }
        catch (Exception ex)
        {
            _logger.Info($"{ex.Message}");
        }
    }
    #endregion
}
