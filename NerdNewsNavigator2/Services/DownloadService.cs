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
    public static bool NotDownloading { get; set; } = !IsDownloading;
    public static string Status { get; set; } = string.Empty;
    public static int Count { get; set; } = 0;
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
                Status = ($"Download Progress: {progressPercentage}%");
            };
            await client.StartDownload();
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
        if (downloaded)
        {
            download.IsDownloaded = true;
            download.IsNotDownloaded = false;
            download.Deleted = false;
            await AddDownloadDatabase(download);
            return true;
        }
        if (CancelDownload)
        {
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
        if (favoriteShows is null)
        {
            return;
        }
        ProccessShow(favoriteShows);
    }
    public static void ProccessShow(List<Favorites> favoriteShows)
    {
        favoriteShows.ForEach(async x =>
        {
            if (CancelDownload)
            {
                Autodownloading = false;
                return;
            }
            var show = await FeedService.GetShows(x.Url, true);
            if (show is null || show.Count == 0)
            {
                return;
            }
            while (Autodownloading)
            {
                Thread.Sleep(5000);
                if (CancelDownload)
                {
                    Autodownloading = false;
                    break;
                }
                Debug.WriteLine("Waiting for download to finish");
            }
            var downloadedshows = await App.PositionData.GetAllDownloads();
            var down = downloadedshows.Exists(y => y.Url == show[0].Url);
            if (!down && !CancelDownload)
            {
                Autodownloading = true;
                var result = await Downloading(show[0]);
                if (result && !CancelDownload)
                {
                    Debug.WriteLine("Download completed");
                    Autodownloading = false;
                    Count++;
#if ANDROID
                    var downloaded = new NotificationRequest
                    {
                        NotificationId = Count,
                        Title = x.Title,
                        Description = "New Episode Downloaded",
                        Android = new AndroidOptions()
                        {
                            IconSmallName = new Plugin.LocalNotification.AndroidOption.AndroidIcon("ic_stat_alarm"),
                        },
                    };
                    if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
                    {
                        await Shell.Current.DisplayAlert("Permission Required", "Notification permission is required for Auto Downloads to work in background. It runs on an hourly schedule.", "Ok");
                        await LocalNotificationCenter.Current.RequestNotificationPermission();
                    }
                    await LocalNotificationCenter.Current.Show(downloaded);
#endif
                }
                else
                {
                    Autodownloading = false;
                }
            }
        });
    }
}
