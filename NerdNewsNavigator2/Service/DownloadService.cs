// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;
/// <summary>
/// A class that manages downloading <see cref="Podcast"/> to local file system.
/// </summary>
public static class DownloadService
{
    /// <summary>
    /// Method Download a show to local file system.
    /// </summary>
    /// <param name="download">Show to be downloaded.</param>
    /// <returns>A <see cref="bool"/> Return true if succesfull, false otherwise.</returns>
    public static async Task<bool> DownloadShow(Download download)
    {
        return await DownloadFile(download.Url);
    }

    /// <summary>
    /// Method Adds Downloaded <see cref="Download"/> to Database.
    /// </summary>
    /// <param name="download">Is the Url of <see cref="Download.Url"/> to Add to datbase.</param> 
    /// <returns>nothing</returns>
    public static async Task<bool> AddDownloadDatabase(Download download)
    {
        var items = await App.PositionData.GetAllDownloads();
        foreach (var item in items)
        {
            if (item.Url == download.Url) { return false; }
        }
        await App.PositionData.AddDownload(download);
        return true;
    }

    /// <summary>
    /// Method Adds Auto Download of <see cref="Podcast"/> to Database.
    /// </summary>
    /// <param name="download"></param>
    /// <returns></returns>
    public static async Task AddAutoDownloadPodcast(Download download)
    {
        var result = await App.PositionData.GetAllPodcasts();
        foreach (var item in result)
        {
            if (item.Url == download.Url)
            {
                await App.PositionData.DeletePodcast(item);
                var podcast = new Podcast
                {
                    Url = item.Url,
                    Title = item.Title,
                    Description = item.Description,
                    PubDate = item.PubDate,
                    Download = true
                };
                await App.PositionData.AddPodcast(podcast);
            }
        }
    }

    /// <summary>
    /// Method Removes Downloaded <see cref="Download"/> from Database.
    /// </summary>
    /// <param name="download"> is the Download to remove from database.</param>
    /// <returns>nothing</returns>
    public static async Task RemoveDownloadFromDatabase(Download download)
    {
        await App.PositionData.DeleteDownload(download);
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
        var filename = GetFileName(url);
        try
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                using (Stream readFrom = await response.Content.ReadAsStreamAsync())
                {
                    string tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
                    using (Stream writeTo = File.Open(tempFile, FileMode.Create))
                    {
                        await readFrom.CopyToAsync(writeTo);
                    }
                }
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}
