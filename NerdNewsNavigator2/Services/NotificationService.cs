// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;
public partial class NotificationService
{
#if ANDROID || IOS
    private int Id { get; set; } = 0;
    private NotificationRequest Request { get; set; } = new();
    private double Progress { get; set; } = 0;
    private readonly Random _random = new();
    public NotificationService()
    {
    }
    public void StartNotifications()
    {
        App.Downloads.DownloadStarted += DownloadStarted;
        App.Downloads.DownloadFinished += DownloadCompleted;
    }
    private async void DownloadCompleted(object sender, DownloadEventArgs e)
    {
        await AfterDownloadNotifications(e.Notification);
        App.Downloads.DownloadStarted -= DownloadStarted;
        App.Downloads.DownloadFinished -= DownloadCompleted;
    }

    private void DownloadStarted(object sender, DownloadEventArgs e)
    {
        Progress = e.Progress;
        DownloadNotifications(e.Notification);
    }
    public async Task Cancel(Show show)
    {
        System.Diagnostics.Debug.WriteLine($"Id is: {Request.NotificationId}, and sending true");
        WeakReferenceMessenger.Default.Send(new NotificationItemMessage(Request.NotificationId, show.Url, show, true));
        Request.Android.ProgressBarProgress = 0;
        Request.Android.Ongoing = false;
        Request.NotificationId = Id;
        Request.Description = "Download cancelled";
        Request.CategoryType = NotificationCategoryType.None;
        DownloadService.Progress = 0;
        await LocalNotificationCenter.Current.Show(Request);
        App.Downloads.DownloadStarted -= DownloadStarted;
        App.Downloads.DownloadFinished -= DownloadCompleted;
    }
    public static async Task CheckNotification()
    {
        if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }
    }
    public void DownloadNotifications(NotificationRequest request)
    {
        _ = Task.Run(async () =>
        {
            CurrentDownloads.IsDownloading = true;
            request.Description = $"Download Progress {(int)Progress}%";
            request.Android.ProgressBarProgress = (int)Progress;
            request.Silent = true;
            request.NotificationId = Id;
            await LocalNotificationCenter.Current.Show(request);
        });
    }
    public async Task AfterDownloadNotifications(NotificationRequest request)
    {
        DownloadService.Progress = 0;
        request.Android.ProgressBarProgress = 100;
        request.Android.Ongoing = false;
        request.Description = "Download Complete";
        request.CategoryType = NotificationCategoryType.Status;
        await LocalNotificationCenter.Current.Show(request);
        App.Downloads.DownloadStarted -= DownloadStarted;
        App.Downloads.DownloadFinished -= DownloadCompleted;
    }

    public async Task<NotificationRequest> NotificationRequests(Show item)
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
        Request = request;
        return request;
    }
#endif
}
