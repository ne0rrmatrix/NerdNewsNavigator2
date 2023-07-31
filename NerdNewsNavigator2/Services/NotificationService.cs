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
    public void StopNotifications()
    {
        App.Downloads.DownloadStarted -= DownloadStarted;
        App.Downloads.DownloadFinished -= DownloadCompleted;
        App.Downloads.DownloadCancelled -= DownloadCancelled;
    }
    private void DownloadCompleted(object sender, DownloadEventArgs e)
    {
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            e.Progress = 0;
            e.Notification.Android.ProgressBarProgress = 100;
            e.Notification.Android.Ongoing = false;
            e.Notification.Description = "Download Complete";
            e.Notification.CategoryType = NotificationCategoryType.Status;
            await LocalNotificationCenter.Current.Show(e.Notification);
        });
        StopNotifications();
    }
    public void StopWaitingForCancel()
    {
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            App.Downloads.DownloadCancelled -= DownloadCancelled;
        });
    }
    public void StartWatiingForCancel()
    {
        App.Downloads.DownloadCancelled += DownloadCancelled;
    }
    private async void DownloadCancelled(object sender, DownloadEventArgs e)
    {
        Debug.WriteLine("Notification cancelled");
        App.Downloads.DownloadCancelled -= DownloadCancelled;
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            e.Notification.Android.ProgressBarProgress = 0;
            e.Notification.Android.Ongoing = false;
            e.Notification.Description = "Download cancelled";
            e.Notification.CategoryType = NotificationCategoryType.None;
            e.Progress = 0;
            await LocalNotificationCenter.Current.Show(e.Notification);
            ThreadPool.QueueUserWorkItem(state =>
            {
                App.Downloads.DownloadCancelled += DownloadCancelled;
            });
        });
    }
    private void DownloadStarted(object sender, DownloadEventArgs e)
    {
        Progress = e.Progress;
        _ = DownloadNotifications(e.Notification);
    }
    public static async Task CheckNotification()
    {
        if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }
    }
    public async Task DownloadNotifications(NotificationRequest request)
    {
        request.Description = $"Download Progress {(int)Progress}%";
        request.Android.ProgressBarProgress = (int)Progress;
        request.Silent = true;
        await LocalNotificationCenter.Current.Show(request);
    }
#endif
}
