// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;
/// <summary>
/// A class that manages downloading <see cref="Podcast"/> to local file system.
/// </summary>
public static class DownloadService
{
    public static bool IsDownloading { get; set; } = false;
    public static bool Autodownloading { get; set; } = false;
    public static bool NotDownloading { get; set; } = !IsDownloading;
    public static string Status { get; set; } = string.Empty;
   
    /// <summary>
    /// Method Adds Downloaded <see cref="Download"/> to Database.
    /// </summary>
    /// <param name="download">Is the Url of <see cref="Download.Url"/> to Add to datbase.</param> 
    /// <returns>nothing</returns>
    public static async Task<bool> AddDownloadDatabase(Download download)
    {
        var items = await App.PositionData.GetAllDownloads();
        if (items.AsEnumerable().Any(x => x.Url == download.Url))
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
                Status = ($"Download Progress: {progressPercentage}%");
            };

            await client.StartDownload();
            return true;
        }
        catch
        {
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
        if (downloaded)
        {
            Debug.WriteLine($"Downloaded file: {download.FileName}");
            await AddDownloadDatabase(download);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Method Auto downloads <see cref="Show"/> from Database.
    /// </summary>
    public static async Task AutoDownload()
    {
        Debug.WriteLine("Trying to start Auto Download");
        var favoriteShows = await App.PositionData.GetAllFavorites();
        var downloadedShows = await App.PositionData.GetAllDownloads();

        if (favoriteShows is null || downloadedShows is null)
        {
            return;
        }
        ProccessShow(favoriteShows, downloadedShows);
    }
    public static void ProccessShow(List<Show> favoriteShows, List<Download> downloadedShows)
    {
        favoriteShows.Where(x => !x.IsDownloaded).ToList().ForEach(async x =>
        {
            var show = await FeedService.GetShows(x.Url, true);
            while (Autodownloading)
            {
                Thread.Sleep(5000);
                Debug.WriteLine("Waiting for download to finish");
            }
            if (!downloadedShows.Any(y => y.Url == x.Url))
            {
                Debug.WriteLine("Downloading ", show.First().Url);
                Autodownloading = true;
                var result = await Downloading(show.First());
                if (result)
                {
                    x.IsDownloaded = true;
                    await App.PositionData.UpdateFavorite(x);
                    Autodownloading = false;
                }
                else
                {
                    Autodownloading = false;
                }
            }
        });
    }
}
