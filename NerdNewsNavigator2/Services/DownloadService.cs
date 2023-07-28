// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;
/// <summary>
/// A class that manages downloading <see cref="Podcast"/> to local file system.
/// </summary>
public class DownloadService
{
    #region Properties
    public static bool CancelDownload { get; set; } = false;
    public static string DownloadFileName { get; set; } = string.Empty;
    public static bool IsDownloading { get; set; } = false;
    public static bool Autodownloading { get; set; } = false;
    public static double Progress { get; set; }
    private CurrentDownloads Downloads { get; set; } = new();
    public static string Status { get; set; } = string.Empty;
    private static readonly ILogger s_log = LoggerFactory.GetLogger(nameof(DownloadService));
    #endregion
    public DownloadService()
    {
    }
    /// <summary>
    /// Get file name from Url <see cref="string"/>
    /// </summary>
    /// <param name="url">A URL <see cref="string"/></param>
    /// <returns>Filename <see cref="string"/> with file extension</returns>
    public static string GetFileName(string url)
    {
        var result = new Uri(url).LocalPath;
        return System.IO.Path.GetFileName(result);

    }

    private static void DeleteFile(string url)
    {
        CancelDownload = true;
        var filename = DownloadService.GetFileName(url);
        var tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
        if (File.Exists(tempFile))
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Download a file to local filesystem from a URL
    /// </summary>
    /// <param name="item"><see cref="Show"/> Url to download file. </param>
    /// <returns><see cref="bool"/> True if download suceeded. False if it fails.</returns>
    public static async Task<bool> DownloadFile(Show item)
    {
        try
        {
            var filename = DownloadService.GetFileName(item.Url);
            var downloadFileUrl = item.Url;
            var favorites = await App.PositionData.GetAllDownloads();
            var tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
            if (File.Exists(tempFile))
            {
                if (favorites?.Find(x => x.Url == item.Url) is null)
                {
                    s_log.Info($"Item is Partially downloaded, Deleting: {filename}");
                    File.Delete(tempFile);
                }
                else
                {
                    s_log.Info("File exists stopping download");
                    return false;
                }
            }
            DownloadFileName = tempFile;
            var destinationFilePath = tempFile;

            using var client = new HttpClientDownloadWithProgress(downloadFileUrl, destinationFilePath);
            client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
            {
                Status = $"Download Progress: {progressPercentage}%";
                Progress = (double)progressPercentage;
            };
            await client.StartDownload();
            if (CancelDownload)
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                    s_log.Info($"Deleting file from cancelled download: {tempFile}");
                }
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            s_log.Error($"{ex.Message}, Deleting file");
            DeleteFile(item.Url);
            return false;
        }
    }

    /// <summary>
    /// Method Auto downloads <see cref="Show"/> from Database.
    /// </summary>
    public async Task AutoDownload()
    {
        CancelDownload = false;
        var favoriteShows = await App.PositionData.GetAllFavorites();
        await ProccessShowAsync(favoriteShows);
    }
    private async Task ProccessShowAsync(List<Favorites> favoriteShows)
    {
        var downloadedShows = await App.PositionData.GetAllDownloads();
        IsDownloading = false;
        _ = Task.Run(() =>
        {
            favoriteShows.ForEach(x =>
            {
                var show = FeedService.GetShows(x.Url, true);
                if (show is not null && !downloadedShows.Exists(y => y.Url == show[0].Url))
                {
                    Downloads.Add(show[0]);
                }
            });
            Downloads.DownloadStarted += DownloadStarted;
            Downloads.DownloadFinished += DownloadCompleted;
            if (Downloads.Shows.Count > 0)
            {
                Downloads.Start(Downloads.Shows[0]);
            }
        });
    }

    private void DownloadCompleted(object sender, DownloadEventArgs e)
    {
        Downloads.DownloadStarted -= DownloadStarted;
        Downloads.DownloadFinished -= DownloadCompleted;
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            Shell.Current.CurrentPage.Title = string.Empty;
        });
        if (e.Shows.Count > 0)
        {
            Downloads.DownloadStarted += DownloadStarted;
            Downloads.DownloadFinished += DownloadCompleted;
            Downloads.Start(e.Shows[0]);
        }
    }

    private void DownloadStarted(object sender, DownloadEventArgs e)
    {
        if (e.Status is null || e.Shows.Count == 0)
        {
            return;
        }
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            Shell.Current.CurrentPage.Title = e.Status;
        });
    }
}
