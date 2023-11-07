// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NerdNewsNavigator2.Interfaces;

namespace NerdNewsNavigator2.Services;
/// <summary>
/// A class that manages downloading <see cref="Podcast"/> to local file system.
/// </summary>
public partial class DownloadService : ObservableObject, IDownloadService
{
    #region Properties

    private CancellationTokenSource CancellationTokenSource { get; set; }
    private static readonly ILogger s_logger = LoggerFactory.GetLogger(nameof(DownloadService));
    private readonly IFileService _fileService;
    private static bool IsDownloading { get; set; }
    public List<Show> Shows { get; private set; }
    private Show Item { get; set; }
#if ANDROID || IOS
    private NotificationRequest Notification { get; set; }
#endif
    #endregion
    public DownloadService(IFileService fileService)
    {
        _fileService = fileService;
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
    public Show Cancel(Show show)
    {
        if (show.Url == Item.Url)
        {
            Shows.Remove(Item);
            IsDownloading = false;
            CancellationTokenSource.Cancel();
            return show;
        }
        Shows.Remove(show);
        return show;
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

        var destinationFilePath = _fileService.GetFileName(item.Url);
        _fileService.DeleteFile(item.Url);

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
    public async Task DownloadSucceeded(Show item)
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
            FileName = _fileService.GetFileName(item.Url)
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

#pragma warning disable CA1822 // Mark members as static - Not rewriting for each device. On Android and IOS it cannot be marked as static. But for windows and Mac it can.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
    private void DownloadFailed(Show item)
#pragma warning restore CA1822 // Mark members as static
    {
        _fileService.DeleteFile(item.Url);
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
