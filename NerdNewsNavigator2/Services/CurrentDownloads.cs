// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;
public partial class CurrentDownloads : ObservableObject
{
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
    public EventHandler<DownloadEventArgs> DownloadCancelled { get; set; }
    public EventHandler<DownloadEventArgs> DownloadFinished { get; set; }
    public EventHandler<DownloadEventArgs> DownloadStarted { get; set; }
    public static bool IsDownloading { get; set; } = false;
    public static bool CancelDownload { get; set; } = false;
    public CurrentDownloads()
    {
        _shows = new();
        _item = new();
#if ANDROID || IOS
        Notify = new();
        Notification = new();
#endif
    }
    public void Add(Show show)
    {
        Shows.Add(show);
    }
    public void CancelAll()
    {
        Shows.Clear();
        CancelDownload = true;
        IsDownloading = false;
    }
    public Show Cancel(string url)
    {
        Cancelled = true;
        var item = Shows.Find(x => x.Url == url) ?? throw new NullReferenceException();
        if (item.Url == Item.Url)
        {
            Cancel();
            CancelDownload = true;
            Shows.Remove(item);
            IsDownloading = false;
            return item;
        }
        Shows.Remove(item);
        return item;
    }
    public void Start(Show show)
    {
        if (IsDownloading || Shows.Count == 0)
        {
            return;
        }
        Item = show;
        CancelDownload = false;
        IsDownloading = true;
        Cancelled = false;
        _ = StartDownload(show);
    }
    private async Task StartDownload(Show item)
    {
#if ANDROID || IOS
        Notify.StartNotifications();
        Notification = await NotificationRequests(item);
#endif
        var result = false;
        result = await DownloadFile(item);
        if (CancelDownload)
        {
            Debug.WriteLine("Download is Cancelled");
            WeakReferenceMessenger.Default.Send(new DownloadItemMessage(false, item.Title, item));
        }
        else if (result)
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
            Cancelled = false;
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
            Description = "Donwloading",
#endif
#if ANDROID
            Description = $"Download Progress {(int)DownloadService.Progress}",
            Android = new AndroidOptions
            {
                IconSmallName = new AndroidIcon("ic_stat_alarm"),
                Ongoing = true,
                ProgressBarProgress = (int)DownloadService.Progress,
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
    private void StartedDownload()
    {
        var args = new DownloadEventArgs
        {
            Status = Status,
            Progress = Progress,
            Cancelled = Cancelled,
#if ANDROID || IOS
            Notification = Notification
#endif
        };
        OnStarted(args);
    }
    private void Cancel()
    {
        var args = new DownloadEventArgs
        {
#if ANDROID || IOS
            Notification = Notification,
            Cancelled = Cancelled,
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
#if ANDROID || IOS
            Notification = Notification
#endif
        };
#if ANDROID || IOS
        Notify.AfterDownloadNotifications(Notification);
#endif
        Debug.WriteLine("Download Completed");
        OnDownloadFinished(args);
        Shows.Remove(Item);
        IsDownloading = false;
        CancelDownload = false;
        if (Shows.Count > 0)
        {
            Thread.Sleep(1000);
            Start(Shows[0]);
        }
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
                Progress = (double)progressPercentage;
                StartedDownload();
            };
            await client.StartDownload();
            if (CancelDownload)
            {
                Status = string.Empty;
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
}
