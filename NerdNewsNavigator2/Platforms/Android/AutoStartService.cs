// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using static Android.OS.PowerManager;

namespace NerdNewsNavigator2.Platforms.Android;

[Service(ForegroundServiceType = ForegroundService.TypeDataSync)]
internal sealed class AutoStartService : Service
{
    #region Properties
    public WakeLock WLock { get; set; }
    public const string NOTIFICATION_CHANNEL_ID = "10276";
    private const int NOTIFICATION_ID = 10923;
    private const string NOTIFICATION_CHANNEL_NAME = "notification";
    private static readonly ILogger s_logger = LoggerFactory.GetLogger(nameof(AutoStartService));
    #endregion

    public AutoStartService()
    {
    }
    #region Foreground Service Methods
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    private void StartForegroundServiceAsync()
    {
        App.AutoDownloadService.Start();

        AcquireWakeLock();
        var intent = new Intent(this, typeof(MainActivity));
        var pendingIntentFlags = Build.VERSION.SdkInt >= BuildVersionCodes.S
            ? PendingIntentFlags.UpdateCurrent |
              PendingIntentFlags.Immutable
            : PendingIntentFlags.UpdateCurrent;

        var pendingIntent = PendingIntent.GetActivity(this, 0, intent, pendingIntentFlags);

        var notificationManager = GetSystemService(NotificationService) as NotificationManager;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            CreateNotificationChannel(notificationManager);

        }

        var notification = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID);
        notification.SetContentIntent(pendingIntent);
        notification.SetAutoCancel(false);
        notification.SetOngoing(true);
        notification.SetContentTitle("AutoDownloader On");
        notification.SetSmallIcon(Resource.Drawable.ic_stat_alarm);
        notification.SetSilent(true);
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
        {
            StartForeground(NOTIFICATION_ID, notification.Build(), ForegroundService.TypeDataSync);
            return;
        }
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            StartForeground(NOTIFICATION_ID, notification.Build());
        }
    }

    private static void CreateNotificationChannel(NotificationManager notificationMnaManager)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var channel = new NotificationChannel(NOTIFICATION_CHANNEL_ID, NOTIFICATION_CHANNEL_NAME,
           NotificationImportance.None);
            notificationMnaManager.CreateNotificationChannel(channel);
#pragma warning restore CA1416 // Validate platform compatibility
        }
    }
    public override IBinder OnBind(Intent intent)
    {
        return null;
    }
    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        s_logger.Info("Staring Auto Download");
        StartForegroundServiceAsync();
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
#pragma warning disable CA1416 // Validate platform compatibility
            StartForegroundService(intent);
#pragma warning restore CA1416 // Validate platform compatibility
        }
    }
    public void AcquireWakeLock()
    {
        WLock?.Release();

        var wakeFlags = WakeLockFlags.Partial;

        var pm = (PowerManager)global::Android.App.Application.Context.GetSystemService(global::Android.Content.Context.PowerService);
        WLock = pm.NewWakeLock(wakeFlags, typeof(AutoStartService).FullName);
        if (!WLock.IsHeld)
        {
            WLock.Acquire();
        }
        var item = WLock.IsHeld;
        s_logger.Info($"Wake Lock On: {item}");

    }
    #endregion

    public override void OnDestroy()
    {
        if (WLock.IsHeld)
        {
            WLock.Release();
            s_logger.Info("Wake lock is being released");
        }
        s_logger.Info($"Wake Lock Status: {WLock.IsHeld}");
        base.OnDestroy();
    }
}
