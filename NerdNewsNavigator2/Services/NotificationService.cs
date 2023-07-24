// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;
public partial class NotificationService
{
#if ANDROID || IOS
    private double Progress { get; set; } = 0;
    public NotificationService()
    {
    }
    public void StartNotifications()
    {
        App.Downloads.DownloadStarted += DownloadStarted;
        App.Downloads.DownloadFinished += DownloadCompleted;
        App.Downloads.DownloadCancelled += DownloadCancelled;
    }
    private void DownloadCompleted(object sender, DownloadEventArgs e)
    {
        AfterDownloadNotifications(e.Notification);
        App.Downloads.DownloadStarted -= DownloadStarted;
        App.Downloads.DownloadFinished -= DownloadCompleted;
        App.Downloads.DownloadCancelled -= DownloadCancelled;
    }

    private void DownloadCancelled(object sender, DownloadEventArgs e)
    {
        e.Notification.Android.ProgressBarProgress = 0;
        e.Notification.Android.Ongoing = false;
        e.Notification.Description = "Download cancelled";
        e.Notification.CategoryType = NotificationCategoryType.None;
        DownloadService.Progress = 0;
        _ = MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await LocalNotificationCenter.Current.Show(e.Notification);
        });
    }

    private void DownloadStarted(object sender, DownloadEventArgs e)
    {
        Progress = e.Progress;
        DownloadNotifications(e.Notification);
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
            await LocalNotificationCenter.Current.Show(request);
        });
    }
    public void AfterDownloadNotifications(NotificationRequest request)
    {
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            DownloadService.Progress = 0;
            request.Android.ProgressBarProgress = 100;
            request.Android.Ongoing = false;
            request.Description = "Download Complete";
            request.CategoryType = NotificationCategoryType.Status;
            await LocalNotificationCenter.Current.Show(request);
            App.Downloads.DownloadStarted -= DownloadStarted;
            App.Downloads.DownloadFinished -= DownloadCompleted;
        });
    }
#endif
}
