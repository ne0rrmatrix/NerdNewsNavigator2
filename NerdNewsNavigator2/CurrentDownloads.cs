// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2;
public partial class CurrentDownloads : ObservableObject
{
    private static string Status { get; set; } = string.Empty;
    private bool _hasStarted = false;
    public EventHandler<DownloadEventArgs> DownloadFinished { get; set; }
    public EventHandler<DownloadEventArgs> DownloadStarted { get; set; }
    [ObservableProperty]
    private List<Show> _shows;
    [ObservableProperty]
    private Show _item;
    public static bool CancelDownload { get; set; } = false;
    public CurrentDownloads()
    {
        _shows = new();
        _item = new();
    }
    public void Add(Show show)
    {
        Shows.Add(show);
    }
    public void CancelAll()
    {
        Shows.Clear();
        CancelDownload = true;
    }
    public Show Cancel(string url)
    {
        var item = Shows.Find(x => x.Url == url) ?? throw new NullReferenceException();
        if (item.Url.Contains(Item.Url))
        {
            CancelDownload = true;
            Debug.WriteLine("Cancel Firing");
            Shows.Remove(item);
            _hasStarted = false;
            var next = Shows[^1];
            if (next is not null)
            {
                Start(next);
            }
            return item;
        }
        Shows.Remove(item);
        return item;
    }
    public void Start(Show show)
    {
        if (_hasStarted || Shows.Count == 0)
        {
            return;
        }
        ThreadPool.QueueUserWorkItem(state =>
        {
            CancelDownload = false;
            Debug.WriteLine("Starting Download");
            _ = StartDownload(show);
        });
    }
    private async Task StartDownload(Show item)
    {
        Item = item;
        _hasStarted = true;
#if ANDROID || IOS
        await SetAndroidNotificationStatus(item);
#endif
        var result = await DownloadFile(item);
        if (!CancelDownload && result)
        {
            Debug.WriteLine("Download Succeeded event triggered");
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
            await App.PositionData.UpdateDownload(download);
            WeakReferenceMessenger.Default.Send(new DownloadItemMessage(true, item.Title));
            Completed(item);
        }
        else
        {
            Debug.WriteLine("Download is Cancelled");
            WeakReferenceMessenger.Default.Send(new DownloadItemMessage(false, item.Title));
        }
    }
    private void StartedDownload()
    {
        var args = new DownloadEventArgs
        {
            Status = Status
        };
        OnStarted(args);
    }
    private void Completed(Show item)
    {
        var args = new DownloadEventArgs
        {
            Item = item
        };
        _hasStarted = false;
        Debug.WriteLine("Download Completed");
        OnDownloadFinished(args);
        Shows.Remove(Item);
        if (Shows.Count > 0)
        {
            _ = StartDownload(Shows[^1]);
        }
    }
    protected virtual void OnStarted(DownloadEventArgs args)
    {
        DownloadStarted?.Invoke(this, args);
    }
    protected virtual void OnDownloadFinished(DownloadEventArgs e)
    {
        DownloadFinished?.Invoke(this, e);
    }
    /// <summary>
    /// Download a file to local filesystem from a URL
    /// </summary>
    /// <param name="item"><see cref="Show"/> Url to download file. </param>
    /// <returns><see cref="bool"/> True if download suceeded. False if it fails.</returns>
    private async Task<bool> DownloadFile(Show item)
    {
        try
        {
            var filename = GetFileName(item.Url);
            var downloadFileUrl = item.Url;
            var favorites = await App.PositionData.GetAllDownloads();
            var tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
            if (File.Exists(tempFile))
            {
                if (favorites?.Where(x => x.Deleted).ToList().Find(y => y.Url == item.Url) is not null || favorites?.Find(x => x.Url == item.Url) is null)
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
            var destinationFilePath = tempFile;
            Debug.WriteLine("Starting download Progress on TaskBar");
            using var client = new HttpClientDownloadWithProgress(downloadFileUrl, destinationFilePath);
            client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
            {
                Status = $"Download Progress: {progressPercentage}%";
                StartedDownload();
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
            CurrentDownloads.DeleteFile(item.Url);
            return false;
        }
    }

    /// <summary>
    /// Get file name from Url <see cref="string"/>
    /// </summary>
    /// <param name="url">A URL <see cref="string"/></param>
    /// <returns>Filename <see cref="string"/> with file extension</returns>
    private static string GetFileName(string url)
    {
        var result = new Uri(url).LocalPath;
        return System.IO.Path.GetFileName(result);

    }
    private static void DeleteFile(string url)
    {
        var filename = GetFileName(url);
        var tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
        if (File.Exists(tempFile))
        {
            File.Delete(tempFile);
        }
    }

#if ANDROID || IOS
    public static async Task SetAndroidNotificationStatus(Show item)
    {
        await NotificationService.CheckNotification();
        if (item is not null)
        {
            var requests = await NotificationService.NotificationRequests(item);
            NotificationService.AfterNotifications(requests);
        }
    }
#endif
}
