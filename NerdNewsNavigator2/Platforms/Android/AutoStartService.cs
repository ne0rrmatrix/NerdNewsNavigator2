// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace NerdNewsNavigator2.Platforms.Android;

[Service]
internal class AutoStartService : Service
{
    public static System.Timers.Timer ATimer { get; set; } = new(60 * 60 * 1000);
    public static CancellationTokenSource CancellationTokenSource { get; set; } = null;

    public const string NOTIFICATION_CHANNEL_ID = "10276";
    private const int NOTIFICATION_ID = 10923;
    private const string NOTIFICATION_CHANNEL_NAME = "notification";
    private readonly IConnectivity _connectivity;
    public AutoStartService()
    {
        _connectivity = MauiApplication.Current.Services.GetService<IConnectivity>();
    }

    /// <summary>
    /// A method that checks if the internet is connected and returns a <see cref="bool"/> as answer.
    /// </summary>
    /// <returns></returns>
    public bool InternetConnected()
    {
        if (_connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            PodcastServices.IsConnected = true;
            return true;
        }
        else
        {
            PodcastServices.IsConnected = false;
            return false;
        }
    }
    private void StartForegroundService()
    {
        if (InternetConnected())
        {
            if (CancellationTokenSource is null)
            {
                var cts = new CancellationTokenSource();
                CancellationTokenSource = cts;
            }
            else if (CancellationTokenSource is not null)
            {
                CancellationTokenSource.Dispose();
                CancellationTokenSource = null;
                var cts = new CancellationTokenSource();
                CancellationTokenSource = cts;
            }
            LongTask(CancellationTokenSource.Token);
        }

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

        this.StartForeground(NOTIFICATION_ID, notification.Build());
    }

    private void CreateNotificationChannel(NotificationManager notificationMnaManager)
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
    public static void LongTask(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            ATimer.Stop();
            ATimer.Elapsed -= new ElapsedEventHandler(OnTimedEvent);
            return;
        }
        ATimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        ATimer.Start();
    }

    private static void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Timed event: {e} Started");
        _ = NerdNewsNavigator2.Services.DownloadService.AutoDownload();
    }

    /// <summary>
    /// A method that Auto starts <see cref="Service"/> for Auto downloads.
    /// </summary>
    public void Start()
    {
        var intent = new Intent(this, typeof(AutoStartService));
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            this.StartForegroundService(intent);
        }
    }
    /// <summary>
    /// A method that stops <see cref="Service"/> for Auto downloads
    /// </summary>
    public void Stop()
    {
        System.Diagnostics.Debug.WriteLine("Stopping Auto Download");
        CancellationTokenSource.Cancel();
        LongTask(CancellationTokenSource.Token);
        CancellationTokenSource?.Dispose();
        CancellationTokenSource = null;
        var intent = new Intent(this, typeof(AutoStartService));
        this.StopService(intent);
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
