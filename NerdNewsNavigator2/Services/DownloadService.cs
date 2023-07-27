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
        _ = Task.Run(async () =>
        {
            var shows = new List<Show>();
            favoriteShows.ForEach(x =>
            {
                var show = FeedService.GetShows(x.Url, true);
                if (show is not null && !downloadedShows.Exists(y => y.Url == show[0].Url))
                {
                    shows.Add(show[0]);
                }
            });
            while (shows.Count > 0)
            {
                if (CancelDownload)
                {
                    shows.Clear();
                    break;
                }
                Debug.WriteLine($"downloading: {shows[0].Title}");
                await StartDownload(shows[0]);
                shows.RemoveAt(0);
            }
        });
    }
    private static async Task StartDownload(Show item)
    {
        Debug.WriteLine($"Starting Download of {item.Title}");
        var result = await DownloadFile(item);
        if (result)
        {
            Debug.WriteLine("Download Completed event triggered");
            Download download = new()
            {
                Title = item.Title,
                Url = item.Url,
                Image = item.Image,
                IsDownloaded = true,
                IsNotDownloaded = false,
                Deleted = false,
                PubDate = item.PubDate,
                Description = item.Description,
                FileName = DownloadService.GetFileName(item.Url)
            };
            Debug.WriteLine($"Download completed: {item.Title}");
            await App.PositionData.UpdateDownload(download);
        }
    }
}
