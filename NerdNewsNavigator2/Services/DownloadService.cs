// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;
/// <summary>
/// A class that manages downloading <see cref="Podcast"/> to local file system.
/// </summary>
public partial class DownloadService
{
    #region Properties
    public CancellationTokenSource CancellationTokenSource { get; set; } = null;
    private static readonly ILogger s_logger = LoggerFactory.GetLogger(nameof(DownloadService));
    #endregion
    public DownloadService()
    {
    }
    #region Download Methods
    /// <summary>
    /// Method Auto downloads <see cref="Show"/> from Database.
    /// </summary>
    public async Task AutoDownload()
    {
        var favoriteShows = await App.PositionData.GetAllFavorites();
        await ProccessShowAsync(favoriteShows);
    }
    private async Task ProccessShowAsync(List<Favorites> favoriteShows)
    {
        if (CancellationTokenSource is not null)
        {
            CancellationTokenSource.Dispose();
            CancellationTokenSource = null;
            var cts = new CancellationTokenSource();
            CancellationTokenSource = cts;
        }
        else if
        (CancellationTokenSource is null)
        {
            var cts = new CancellationTokenSource();
            CancellationTokenSource = cts;
        }
        var downloadedShows = await App.PositionData.GetAllDownloads();
        _ = Task.Run(() =>
        {

            if (App.Downloads.Shows.Count > 0)
            {
                s_logger.Info("Manual dowload in progress. Cancelling auto download");
                return;
            }
            favoriteShows.ForEach(x =>
            {
                var show = FeedService.GetShows(x.Url, true);
                if (show is not null && !downloadedShows.Exists(y => y.Url == show[0].Url))
                {
                    App.Downloads.Add(show[0]);
                }
            });
            App.Downloads.DownloadStarted += DownloadStarted;
            App.Downloads.DownloadFinished += DownloadCompleted;
            if (App.Downloads.Shows.Count > 0)
            {
                s_logger.Info("Starting to download favorite shows");
#if ANDROID || IOS
                _ = App.Downloads.Start(App.Downloads.Shows[0]);
#else
                App.Downloads.Start(App.Downloads.Shows[0]);
#endif
            }
        });
    }
    #endregion

    #region Events
    private void DownloadStarted(object sender, DownloadEventArgs e)
    {
        if (CancellationTokenSource.IsCancellationRequested)
        {
            App.Downloads.DownloadStarted -= DownloadStarted;
            App.Downloads.DownloadFinished -= DownloadCompleted;
            s_logger.Info("Stopping auto download");
            App.Downloads.CancelAll();
            if (CancellationTokenSource is not null)
            {
                CancellationTokenSource.Cancel();
                CancellationTokenSource?.Dispose();
                CancellationTokenSource = null;
            }
        }
    }

    private void DownloadCompleted(object sender, DownloadEventArgs e)
    {
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            Shell.Current.CurrentPage.Title = string.Empty;
        });
        if (e.Shows.Count > 0)
        {
#if ANDROID || IOS
            App.Downloads.Notify.StartNotifications();
            _ = App.Downloads.Start(e.Shows[0]);
#else
            App.Downloads.Start(e.Shows[0]);
#endif
        }
        if (e.Shows.Count == 0)
        {
            App.Downloads.DownloadStarted -= DownloadStarted;
            App.Downloads.DownloadFinished -= DownloadCompleted;
        }
    }
    #endregion
}
