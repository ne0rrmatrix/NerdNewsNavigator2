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
    public bool Running { get; set; } = true;
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
        Running = true;
        StartForegroundService();
        if (!InternetConnected())
        {
            Running = false;
        }
        _ = Task.Run(async () =>
        {
            while (Running)
            {
                Thread.Sleep(5000);
                await Services.DownloadService.AutoDownload();
                Thread.Sleep(1000 * 60 * 60);
            }
        });

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
            this.StartForegroundService(intent);
        }

    }
    /// <summary>
    /// A method that stops <see cref="Service"/> for Auto downloads
    /// </summary>
    public void Stop()
    {
        var intent = new Intent(this, typeof(AutoStartService));
        Running = false;
        this.StopService(intent);
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        Running = false;
    }
}
