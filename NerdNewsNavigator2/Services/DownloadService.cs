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
    public static bool IsDownloading { get; set; } = false;
    public static bool Autodownloading { get; set; } = false;
    public static double Progress { get; set; }
    public static bool NotDownloading { get; set; } = !IsDownloading;
    public static string Status { get; set; } = string.Empty;
    #endregion

    /// <summary>
    /// Method Adds Downloaded <see cref="Download"/> to Database.
    /// </summary>
    /// <param name="download">Is the Url of <see cref="Download"/> to Add to datbase.</param> 
    /// <returns>nothing</returns>
    public static async Task<bool> AddDownloadDatabase(Download download)
    {
        var items = await App.PositionData.GetAllDownloads();
        if (items.Exists(x => x.Url == download.Url))
        {
            return false;
        }
        await App.PositionData.AddDownload(download);
        return true;
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
    /// <param name="url"><see cref="string"/> Url to download file. </param>
    /// <returns><see cref="bool"/> True if download suceeded. False if it fails.</returns>
    public static async Task<bool> DownloadFile(string url)
    {
        try
        {
            var filename = GetFileName(url);
            var downloadFileUrl = url;
            var favorites = await App.PositionData.GetAllDownloads();
            var tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
            if (File.Exists(tempFile))
            {
                if (favorites?.Find(x => x.Url == url) is null)
                {
                    Debug.WriteLine($"Item is Partially downloaded, Deleting: {url}");
                    File.Delete(tempFile);
                }
                else
                {
                    Debug.WriteLine("File exists stopping download");
                    return false;
                }
            }
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
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.Message}, Deleting file");
            DeleteFile(url);
            return false;
        }
    }

    /// <summary>
    /// A method that download a show to device.
    /// </summary>
    /// <param name="show"></param>
    /// <returns></returns>
    public static async Task<bool> Downloading(Show show)
    {
        Download download = new()
        {
            Title = show.Title,
            Url = show.Url,
            Image = show.Image,
            PubDate = show.PubDate,
            Description = show.Description,
            FileName = GetFileName(show.Url)
        };

        var downloaded = await DownloadFile(download.Url);
        if (downloaded && !CancelDownload)
        {
            download.IsDownloaded = true;
            download.IsNotDownloaded = false;
            download.Deleted = false;
            await AddDownloadDatabase(download);
            return true;
        }

        if (CancelDownload)
        {
            Debug.WriteLine("Deleting file");
            DeleteFile(download.Url);
            return false;
        }
        return false;
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
            favoriteShows.ForEach(x =>
            {
                var show = FeedService.GetShows(x.Url, true);
                while (IsDownloading)
                {
                    Thread.Sleep(8000);
                    if (CancelDownload)
                    {
                        IsDownloading = false;
                        CancelDownload = false;
                        return;
                    }
                    Debug.WriteLine("Waiting for download to finish");
                }
                if (show is null || show.Count == 0 || CancelDownload)
                {
                    Autodownloading = false;
                    return;
                }
                _ = Task.Run(async () =>
                {
                    await ProcessDownloadAsync(downloadedShows, show[0]);
                    IsDownloading = false;
                });
            });
        });
    }

    private static async Task ProcessDownloadAsync(List<Download> downloadedShows, Show show)
    {
        if (!downloadedShows.Exists(y => y.Url == show.Url))
        {
            IsDownloading = true;
#if ANDROID
            _ = Task.Run(async () =>
            {
                await NotificationService.CheckNotification();
                var requests = await NotificationService.NotificationRequests(show);
                NotificationService.AfterNotifications(requests);
            });
#endif
            await Downloading(show);
        }
    }
}
