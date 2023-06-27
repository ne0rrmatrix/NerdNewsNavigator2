// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using static Android.OS.PowerManager;

namespace NerdNewsNavigator2.Platforms.Android;

[Service]
internal class AutoStartService : Service
{
    #region Properties
    private AutoDownloadService AutoDownloadService { get; set; } = new();
    public const string NOTIFICATION_CHANNEL_ID = "10276";
    private const int NOTIFICATION_ID = 10923;
    private const string NOTIFICATION_CHANNEL_NAME = "notification";
    #endregion

    public AutoStartService()
    {
    }
    private void StartForegroundService()
    {
        AutoDownloadService.AcquireWakeLock();
        if (AutoDownloadService.CancellationTokenSource is null)
        {
            var cts = new CancellationTokenSource();
            AutoDownloadService.CancellationTokenSource = cts;
        }
        else if (AutoDownloadService.CancellationTokenSource is not null)
        {
            AutoDownloadService.CancellationTokenSource.Dispose();
            AutoDownloadService.CancellationTokenSource = null;
            var cts = new CancellationTokenSource();
            AutoDownloadService.CancellationTokenSource = cts;
        }
        AutoDownloadService.LongTask(AutoDownloadService.CancellationTokenSource.Token);

        var intent = new Intent(this, typeof(MainActivity));
        var pendingIntentFlags = Build.VERSION.SdkInt >= BuildVersionCodes.S
            ? PendingIntentFlags.UpdateCurrent |
              PendingIntentFlags.Mutable
            : PendingIntentFlags.UpdateCurrent;
        var pendingIntent = PendingIntent.GetActivity(this, 0, intent, pendingIntentFlags);

        var notifcationManager = GetSystemService(NotificationService) as NotificationManager;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            CreateNotificationChannel(notifcationManager);

        }

        var notification = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID);
        notification.SetContentIntent(pendingIntent);
        notification.SetAutoCancel(false);
        notification.SetOngoing(true);
        notification.SetContentTitle("AutoDownloader On");
        notification.SetSmallIcon(Resource.Drawable.ic_stat_alarm);
        notification.SetSilent(true);

        StartForeground(NOTIFICATION_ID, notification.Build());
    }

    private static void CreateNotificationChannel(NotificationManager notificationMnaManager)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(NOTIFICATION_CHANNEL_ID, NOTIFICATION_CHANNEL_NAME,
           NotificationImportance.None);
            notificationMnaManager.CreateNotificationChannel(channel);
        }
    }
    public override IBinder OnBind(Intent intent)
    {
        return null;
    }
    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        System.Diagnostics.Debug.WriteLine("Staring Auto Download");
        StartForegroundService();
        return StartCommandResult.Sticky;
    }

    /// <summary>
    /// A method that Auto starts <see cref="Service"/> for Auto downloads.
    /// </summary>
    public void Start()
    {
        var intent = new Intent(this, typeof(AutoStartService));
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            StartForegroundService(intent);
        }
    }
    public override void OnDestroy()
    {
        if (AutoDownloadService.WLock.IsHeld)
        {
            AutoDownloadService.WLock.Release();
        }
        System.Diagnostics.Debug.WriteLine($"Wake Lock Status: {AutoDownloadService.WLock.IsHeld}");
        if (AutoDownloadService.CancellationTokenSource is not null)
        {
            AutoDownloadService.CancellationTokenSource = null;
        }
        AutoDownloadService.CancellationTokenSource?.Dispose();
        base.OnDestroy();
    }
}
