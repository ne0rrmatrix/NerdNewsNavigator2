// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;
/// <summary>
/// A class that manages downloading <see cref="Podcast"/> to local file system.
/// </summary>
public partial class DownloadService : ObservableObject
{
    #region Properties
    private CancellationTokenSource CancellationTokenSource { get; set; } = null;
    private static readonly ILogger s_logger = LoggerFactory.GetLogger(nameof(DownloadService));
    private static bool IsDownloading { get; set; } = false;
    public List<Show> Shows { get; private set; }
    private Show Item { get; set; }
#if ANDROID || IOS
    private int Id { get; set; } = 0;
    private NotificationRequest Notification { get; set; }
#endif
    #endregion
    public DownloadService()
    {
        SetToken();
        Shows = new();
        Item = new();
#if ANDROID || IOS
        Notification = new();
#endif
    }

    public void Add(Show show)
    {
        Shows.Add(show);
    }
    public void CancelAll()
    {
        CancellationTokenSource.Cancel();
        Shows.Clear();
        IsDownloading = false;
    }
    public Show Cancel(string url)
    {
        Debug.WriteLine("Cancel called");
        var item = Shows.Find(x => x.Url == url) ?? throw new NullReferenceException();
        if (item.Url == Item.Url)
        {
            Shows.Remove(item);
            IsDownloading = false;
            CancellationTokenSource.Cancel();
            return item;
        }
        Shows.Remove(item);
        return item;
    }

    #region Download Methods

#if ANDROID || IOS
    public async Task Start(Show show)
    {
        if (IsDownloading)
        {
            s_logger.Info("Download is being added to Que");
            return;
        }
        Notification = await App.NotificationService.NotificationRequests(show);
        ThreadPool.QueueUserWorkItem(async state =>
        {
            await StartDownload(show);
        });
    }
#else
    public void Start(Show show)
    {
        if (IsDownloading)
        {
            s_logger.Info("Download is being added to Que");
            return;
        }
        ThreadPool.QueueUserWorkItem(async state =>
        {
            await StartDownload(show);
        });
    }
#endif
    private async Task StartDownload(Show item)
    {
        s_logger.Info($"Starting Download of {item.Title}");
        SetToken();
        Item = item;
        IsDownloading = true;
        var result = await DownloadFile(item);

        if (result)
        {
            s_logger.Info("Download Completed event triggered");
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
                FileName = FileService.GetFileName(item.Url)
            };
            s_logger.Info($"Download completed: {item.Title}");
            await App.PositionData.UpdateDownload(download);
            Shows.Remove(item);
            IsDownloading = false;

            WeakReferenceMessenger.Default.Send(new DownloadItemMessage(true, item.Title, item));
#if ANDROID || IOS
            App.Downloads.Completed(item, Notification);
#else
            App.Downloads.Completed(item);
#endif
        }
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
            var favorites = await App.PositionData.GetAllDownloads();
            var tempFile = FileService.GetFileName(item.Url);
            if (File.Exists(tempFile))
            {
                if (favorites?.Where(x => x.Deleted).ToList().Find(y => y.Url == item.Url) is not null || favorites?.Find(x => x.Url == item.Url) is null)
                {
                    s_logger.Info($"Item is Partially downloaded, Deleting: {tempFile}");
                    File.Delete(tempFile);
                }
                else
                {
                    s_logger.Info("File exists stopping download");
                    return false;
                }
            }
            var destinationFilePath = tempFile;
            s_logger.Info("Starting download Progress on TaskBar");
            using var client = new HttpClientDownloadWithProgress(item.Url, destinationFilePath);
            client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage, tokenSource) =>
            {
                if (CancellationTokenSource.IsCancellationRequested)
                {
                    client.DownloadCancel.Cancel(false);
                }
#if ANDROID || IOS
                var title = $" Download Progress: {progressPercentage}%";
                App.Downloads.StartedDownload(title, item, Notification);
#else
                var title = $"        Download Progress: {progressPercentage}%";
                App.Downloads.StartedDownload(title, item);
#endif
            };
            if (!CancellationTokenSource.IsCancellationRequested)
            {
                await client.StartDownload();
            }
            if (CancellationTokenSource.IsCancellationRequested)
            {
                FileService.DeleteFile(item.Url);
#if ANDROID || IOS
                App.Downloads.Cancel(item, Notification);
#else
                App.Downloads.Cancel(item);
#endif
                _ = MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Toast.Make("Download cancelled", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
                });
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            s_logger.Info($"{ex.Message}, Deleting file");
            FileService.DeleteFile(item.Url);
            _ = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Toast.Make($"Download error: {ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
            });
            return false;
        }
    }
    #endregion
    private void SetToken()
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
    }
}
