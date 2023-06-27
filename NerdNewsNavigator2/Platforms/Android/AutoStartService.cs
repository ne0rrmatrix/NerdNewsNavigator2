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
    private WakeLock _wakeLock;
    private string Status { get; set; }
    private string WifiOnlyDownloading { get; set; }
    private System.Timers.Timer ATimer { get; set; } = new(60 * 60 * 1000);
    public CancellationTokenSource CancellationTokenSource { get; set; } = null;

    public const string NOTIFICATION_CHANNEL_ID = "10276";
    private const int NOTIFICATION_ID = 10923;
    private const string NOTIFICATION_CHANNEL_NAME = "notification";
    #endregion

    public AutoStartService()
    {
        WifiOnlyDownloading = Preferences.Default.Get("WifiOnly", "No");
        Status = string.Join(", ", Connectivity.Current.ConnectionProfiles);
    }
    private void StartForegroundService()
    {
        AcquireWakeLock();
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
    private void AcquireWakeLock()
    {
        _wakeLock?.Release();

        var wakeFlags = WakeLockFlags.Partial;

        var pm = (PowerManager)global::Android.App.Application.Context.GetSystemService(global::Android.Content.Context.PowerService);
        _wakeLock = pm.NewWakeLock(wakeFlags, typeof(AutoStartService).FullName);
        if (!_wakeLock.IsHeld)
        {
            _wakeLock.Acquire();
        }
        var item = _wakeLock.IsHeld;
        System.Diagnostics.Debug.WriteLine($"Wake Lock On: {item}");

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
    public void LongTask(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            ATimer.Stop();
            Connectivity.Current.ConnectivityChanged -= GetCurrentConnectivity;
            ATimer.Elapsed -= new System.Timers.ElapsedEventHandler(OnTimedEvent);
            return;
        }
        ATimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
        Connectivity.Current.ConnectivityChanged += GetCurrentConnectivity;
        ATimer.Start();
    }

    private void GetCurrentConnectivity(object sender, ConnectivityChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Connection status has changed");
        Status = string.Join(", ", Connectivity.Current.ConnectionProfiles);
        System.Diagnostics.Debug.WriteLine(Status);
        WifiOnlyDownloading = Preferences.Default.Get("WifiOnly", "No");
    }
    private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
    {
        WifiOnlyDownloading = Preferences.Default.Get("WifiOnly", "No");

        var connectionStatus = Connectivity.Current.ConnectionProfiles;
        connectionStatus.ToList().ForEach(item =>
        {
            switch (item)
            {
                case ConnectionProfile.WiFi:
                case ConnectionProfile.Ethernet:
                    System.Diagnostics.Debug.WriteLine($"Timed event: {e} Started");
                    _ = NerdNewsNavigator2.Services.DownloadService.AutoDownload();
                    break;
                case ConnectionProfile.Cellular:
                    if (WifiOnlyDownloading == "No")
                    {
                        System.Diagnostics.Debug.WriteLine($"Timed event: {e} Started");
                        _ = NerdNewsNavigator2.Services.DownloadService.AutoDownload();
                    }
                    break;
            }
        });
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
        if (_wakeLock.IsHeld)
        {
            _wakeLock.Release();
        }
        System.Diagnostics.Debug.WriteLine($"Wake Lock Status: {_wakeLock.IsHeld}");
        if (CancellationTokenSource is not null)
        {
            CancellationTokenSource = null;
        }
        CancellationTokenSource?.Dispose();
        base.OnDestroy();
    }
}
