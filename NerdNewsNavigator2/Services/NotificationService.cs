// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;
public partial class NotificationService
{
#if ANDROID || IOS

    private readonly Random _random = new();
    public NotificationService()
    {
    }
    public void StartNotifications()
    {
        App.Downloads.DownloadFinished += DownloadCompleted;
        App.Downloads.DownloadCancelled += DownloadCancelled;
    }
    public void StopNotifications()
    {
        App.Downloads.DownloadFinished -= DownloadCompleted;
        App.Downloads.DownloadCancelled -= DownloadCancelled;
    }
    private void DownloadCompleted(object sender, DownloadEventArgs e)
    {
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            e.Progress = 0;
            e.Notification.Android.Ongoing = false;
            e.Notification.Description = "Download Complete";
            e.Notification.CategoryType = NotificationCategoryType.Status;
            await LocalNotificationCenter.Current.Show(e.Notification);
        });
        StopNotifications();
    }
    public async Task<NotificationRequest> NotificationRequests(Show item)
    {
        var id = _random.Next();
        WeakReferenceMessenger.Default.Send(new NotificationItemMessage(id, item.Url, item, false));
        var request = new Plugin.LocalNotification.NotificationRequest
        {
            NotificationId = id,
            Title = item.Title,
            CategoryType = NotificationCategoryType.Status,
            Description = "Downloading",
#if ANDROID
            Android = new AndroidOptions
            {
                IconSmallName = new AndroidIcon("ic_stat_alarm"),
                Ongoing = true,
                Color =
                {
                    ResourceName = "colorPrimary"
                },
                AutoCancel = true,
            },
#endif
        };
        await LocalNotificationCenter.Current.Show(request);
        return request;
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
        });
    }
#endif
}
