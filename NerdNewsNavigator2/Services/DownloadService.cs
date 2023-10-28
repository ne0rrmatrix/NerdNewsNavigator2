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
        Shows = [];
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
    public async Task Start(Show item)
    {
        if (IsDownloading)
        {
            s_logger.Info("Download is being added to Que");
            return;
        }
        SetToken();
        Item = item;
        IsDownloading = true;

        var destinationFilePath = FileService.GetFileName(item.Url);
        FileService.DeleteFile(item.Url);

#if ANDROID || IOS
        Notification = await App.NotificationService.NotificationRequests(item);
#endif
        s_logger.Info($"Starting Download of {item.Title}");

        using var client = new HttpClientDownloadWithProgress(item.Url, destinationFilePath);
        UpdateDownloadStatus(client, item);
        if (await StartClient(client) && !CancellationTokenSource.IsCancellationRequested)
        {
            await DownloadSucceeded(item);
            return;
        }
        DownloadFailed(item);
    }
    private async Task<bool> StartClient(HttpClientDownloadWithProgress client)
    {
        try
        {
            await client.StartDownload();
            if (CancellationTokenSource.IsCancellationRequested)
            {
                return false;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
    private async Task DownloadSucceeded(Show item)
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

    private void DownloadFailed(Show item)
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
    }
    private void UpdateDownloadStatus(HttpClientDownloadWithProgress client, Show item)
    {
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
    }
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
