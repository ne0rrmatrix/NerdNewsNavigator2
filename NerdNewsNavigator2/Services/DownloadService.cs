// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;
/// <summary>
/// A class that manages downloading <see cref="Podcast"/> to local file system.
/// </summary>
public static class DownloadService
{
    #region Properties
    public static bool CancelDownload { get; set; } = false;
    public static string DownloadFileName { get; set; } = string.Empty;
    public static bool IsDownloading { get; set; } = false;
    public static bool Autodownloading { get; set; } = false;
    public static double Progress { get; set; }
    public static string Status { get; set; } = string.Empty;
    #endregion

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

    public static void DeleteFile(string url)
    {
        CancelDownload = true;
        var filename = GetFileName(url);
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
            var filename = GetFileName(item.Url);
            var downloadFileUrl = item.Url;
            var favorites = await App.PositionData.GetAllDownloads();
            var tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
            if (File.Exists(tempFile))
            {
                if (favorites?.Find(x => x.Url == item.Url) is null)
                {
                    Debug.WriteLine($"Item is Partially downloaded, Deleting: {filename}");
                    File.Delete(tempFile);
                }
                else
                {
                    Debug.WriteLine("File exists stopping download");
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
                    Debug.WriteLine($"Deleting file from cancelled download: {tempFile}");
                }
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.Message}, Deleting file");
            DeleteFile(item.Url);
            return false;
        }
    }

    /// <summary>
    /// Method Auto downloads <see cref="Show"/> from Database.
    /// </summary>
    public static async Task AutoDownload()
    {
        CancelDownload = false;
        var favoriteShows = await App.PositionData.GetAllFavorites();
        await ProccessShowAsync(favoriteShows);
    }
    private static async Task ProccessShowAsync(List<Favorites> favoriteShows)
    {
        var downloadedShows = await App.PositionData.GetAllDownloads();
        IsDownloading = false;
        _ = Task.Run(() =>
        {
            favoriteShows.Where(y => !downloadedShows.Exists(d => d.Url == y.Url)).ToList().ForEach(async x =>
            {
                var show = FeedService.GetShows(x.Url, true);
                while (IsDownloading)
                {
                    Thread.Sleep(8000);
                    if (CancelDownload)
                    {
                        IsDownloading = false;
                        return;
                    }
                    Debug.WriteLine("Waiting for download to finish");
                }
                if (show is null || show.Count == 0 || CancelDownload)
                {
                    Autodownloading = false;
                    return;
                }
                else if (!downloadedShows.Exists(y => y.Title == x.Title))
                {
                    await ProcessDownloadAsync(downloadedShows, show[0]);
                }
                IsDownloading = false;
            });
        });
    }

    private static async Task ProcessDownloadAsync(List<Download> downloadedShows, Show show)
    {
        var item = downloadedShows.Find(x => x.Url == show.Url);
        if (item is null)
        {
            IsDownloading = true;
            var result = await DownloadFile(show);
            if (result)
            {
                Download download = new()
                {
                    Title = show.Title,
                    Url = show.Url,
                    Image = show.Image,
                    IsDownloaded = true,
                    IsNotDownloaded = false,
                    Deleted = false,
                    PubDate = show.PubDate,
                    Description = show.Description,
                    FileName = GetFileName(show.Url)
                };
                await App.PositionData.AddDownload(download);
            }
        }
    }
}
