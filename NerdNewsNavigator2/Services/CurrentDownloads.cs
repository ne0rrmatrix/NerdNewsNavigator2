// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;
public partial class CurrentDownloads : ObservableObject
{
    #region Properties
    private static bool IsDownloading { get; set; } = false;
    [ObservableProperty]
    private List<Show> _shows;
    [ObservableProperty]
    private Show _item;
    [ObservableProperty]
    private string _status = string.Empty;
    [ObservableProperty]
    private bool _cancelled = false;
#if ANDROID || IOS
    [ObservableProperty]
    private NotificationService _notify;
    private int Id { get; set; } = 0;
    private readonly Random _random = new();
    [ObservableProperty]
    private NotificationRequest _notification;
#endif
    private static double Progress { get; set; }

    private static readonly ILogger s_logger = LoggerFactory.GetLogger(nameof(CurrentDownloads));
    public EventHandler<DownloadEventArgs> DownloadCancelled { get; set; }
    public EventHandler<DownloadEventArgs> DownloadFinished { get; set; }
    public EventHandler<DownloadEventArgs> DownloadStarted { get; set; }

    #endregion
    public CurrentDownloads()
    {
        _shows = new();
        _item = new();
#if ANDROID || IOS
        Notify = new();
        Notification = new();
#endif
    }
    #region Methods
    public void Add(Show show)
    {
        Shows.Add(show);
    }
    public void CancelAll()
    {
        Cancelled = true;
        Shows.Clear();
        IsDownloading = false;
    }
    public void Cancelling()
    {
        Cancelled = true;
        Shows.Remove(Item);
        IsDownloading = false;
    }
    public Show Cancel(string url)
    {
        Debug.WriteLine("Cancel called");
        var item = Shows.Find(x => x.Url == url) ?? throw new NullReferenceException();
        if (item.Url == Item.Url)
        {
            Cancelled = true;
            Shows.Remove(item);
            IsDownloading = false;
            return item;
        }
        Shows.Remove(item);
        return item;
    }
    public void Start(Show show)
    {
        s_logger.Info($"Staring show: {show.Title}");
        if (IsDownloading)
        {
            s_logger.Info("Download is not starting IDownloading is true");
            return;
        }
#if ANDROID || IOS
        Notify.StartNotifications();
        _ = NotificationRequests(show);
#endif
        ThreadPool.QueueUserWorkItem(async state =>
        {
            await StartDownload(show);
        });
    }
    private async Task StartDownload(Show item)
    {
        Item = item;
        IsDownloading = true;
        Cancelled = false;
        s_logger.Info($"Starting Download of {item.Title}");
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
                FileName = Services.DownloadService.GetFileName(item.Url)
            };
            s_logger.Info($"Download completed: {item.Title}");
            await App.PositionData.UpdateDownload(download);
            Shows.Remove(item);
            IsDownloading = false;
            WeakReferenceMessenger.Default.Send(new DownloadItemMessage(true, item.Title, item));
            Completed(item);
        }
    }

#if ANDROID || IOS
    private async Task<NotificationRequest> NotificationRequests(Show item)
    {
        Id = _random.Next();
        WeakReferenceMessenger.Default.Send(new NotificationItemMessage(Id, item.Url, item, false));
        var request = new Plugin.LocalNotification.NotificationRequest
        {
            NotificationId = Id,
            Title = item.Title,
            CategoryType = NotificationCategoryType.Progress,
#if IOS
            Description = "Downloading",
#endif
#if ANDROID
            Description = $"Download Progress {(int)Progress}",
            Android = new AndroidOptions
            {
                IconSmallName = new AndroidIcon("ic_stat_alarm"),
                Ongoing = true,
                ProgressBarProgress = (int)Progress,
                IsProgressBarIndeterminate = false,
                Color =
                {
                    ResourceName = "colorPrimary"
                },
                AutoCancel = true,
                ProgressBarMax = 100,
            },
#endif
        };
        await LocalNotificationCenter.Current.Show(request);
        Notification = request;
        return request;
    }
#endif

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
                    s_logger.Info($"Item is Partially downloaded, Deleting: {filename}");
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
            using var client = new HttpClientDownloadWithProgress(downloadFileUrl, destinationFilePath);
            client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage, tokenSource) =>
            {
                Status = $"        Download Progress: {progressPercentage}%";
                Progress = (double)progressPercentage;
                StartedDownload();
                if (Cancelled)
                {
                    client.DownloadCancel.Cancel();
                }
            };
            if (!Cancelled)
            {
                await client.StartDownload();
            }
            if (Cancelled)
            {
                DeleteFile(item.Url);
                Cancel(item);
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
            DeleteFile(item.Url);
            _ = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Toast.Make($"Download error: {ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
            });
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
            s_logger.Info($"Deleted file {tempFile}");
        }
    }
    #endregion

    #region EventArgs
    private void StartedDownload()
    {
        var args = new DownloadEventArgs
        {
            Status = Status,
            Progress = Progress,
            Cancelled = Cancelled,
            Item = Item,
            Shows = Shows,
#if ANDROID || IOS
            Notification = Notification
#endif
        };
        OnStarted(args);
    }
    private void Cancel(Show item)
    {
        var args = new DownloadEventArgs
        {
            Item = item,
            Status = Status,
            Cancelled = Cancelled,
            Shows = Shows,
            Progress = Progress,
#if ANDROID || IOS
            Notification = Notification
#endif
        };
        OnCancelled(args);
    }
    private void Completed(Show item)
    {
        var args = new DownloadEventArgs
        {
            Item = item,
            Status = Status,
            Cancelled = Cancelled,
            Progress = Progress,
            Shows = Shows,
#if ANDROID || IOS
            Notification = Notification
#endif
        };
        OnDownloadFinished(args);
    }
    protected virtual void OnCancelled(DownloadEventArgs args)
    {
        DownloadCancelled?.Invoke(this, args);
    }
    protected virtual void OnStarted(DownloadEventArgs args)
    {
        DownloadStarted?.Invoke(this, args);
    }
    protected virtual void OnDownloadFinished(DownloadEventArgs e)
    {
        DownloadFinished?.Invoke(this, e);
    }
    #endregion
}
