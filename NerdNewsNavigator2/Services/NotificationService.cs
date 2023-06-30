// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;
public static class NotificationService
{
#if ANDROID || IOS
    private static int Id { get; set; } = 0;
    public static async Task CheckNotification()
    {
        if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }
    }
    public static void AfterNotifications(NotificationRequest request)
    {
        _ = Task.Run(async () =>
        {
            DownloadService.IsDownloading = true;
            while (DownloadService.IsDownloading)
            {
                if (App.Stop || DownloadService.CancelDownload)
                {
                    break;
                }
#if ANDROID
                request.Description = $"Download Progress {(int)DownloadService.Progress}%";
                request.Android.ProgressBarProgress = (int)DownloadService.Progress;
                request.Silent = true;
                request.NotificationId = Id;
                await LocalNotificationCenter.Current.Show(request);
#endif
                Thread.Sleep(1000);
            }
            if (DownloadService.CancelDownload)
            {
                System.Diagnostics.Debug.WriteLine($"Id is: {request.NotificationId}, and sending true");
                WeakReferenceMessenger.Default.Send(new NotificationItemMessage(request.NotificationId, request.Title, true));
                request.Android.ProgressBarProgress = 0;
                request.Android.Ongoing = false;
                request.NotificationId = Id;
                request.Description = "Download cancelled";
                request.CategoryType = NotificationCategoryType.None;
                DownloadService.Progress = 0;
                await LocalNotificationCenter.Current.Show(request);
            }
            else
            {
                DownloadService.Progress = 0;
                request.Android.ProgressBarProgress = 100;
                request.Android.Ongoing = false;
                request.Description = "Download Complete";
                request.CategoryType = NotificationCategoryType.None;
                if (!App.Stop)
                {
                    await LocalNotificationCenter.Current.Show(request);
                }
            }
        });
    }
    public static async Task<NotificationRequest> NotificationRequests(Show item)
    {
        Id += 1;
        WeakReferenceMessenger.Default.Send(new NotificationItemMessage(Id, item?.Url, false));
        var request = new Plugin.LocalNotification.NotificationRequest
        {
            NotificationId = Id,
            Title = item?.Title,
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
        return request;
    }
#endif
}
