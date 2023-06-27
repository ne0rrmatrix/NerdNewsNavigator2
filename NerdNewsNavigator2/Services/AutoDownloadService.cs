﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if ANDROID
using Android.OS;
using static Android.OS.PowerManager;
#endif

namespace NerdNewsNavigator2.Services;

public class AutoDownloadService
{
    private string Status { get; set; }
    private string WifiOnlyDownloading { get; set; }
#if ANDROID
    public WakeLock WLock { get; set; }
#endif
    private System.Timers.Timer ATimer { get; set; } = new(60 * 60 * 1000);
    public CancellationTokenSource CancellationTokenSource { get; set; } = null;
    public AutoDownloadService()
    {
        Status = string.Join(", ", Connectivity.Current.ConnectionProfiles);
        WifiOnlyDownloading = Preferences.Default.Get("WifiOnly", "No");
        Task.Run(() =>
        {
            var profiles = Connectivity.Current.ConnectionProfiles;
            var start = Preferences.Default.Get("start", false);
            if (!start)
            {
                System.Diagnostics.Debug.WriteLine("Auto Downloader is disabled in settings");
                return;
            }
            if (profiles.Contains(ConnectionProfile.WiFi) || WifiOnlyDownloading == "No")
            {
                Thread.Sleep(60 * 1000);
                System.Diagnostics.Debug.WriteLine("Auto download now");
                _ = DownloadService.AutoDownload();
            }
        });
    }
#if ANDROID
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
        System.Diagnostics.Debug.WriteLine($"Wake Lock On: {item}");

    }
#endif

    /// <summary>
    /// A method that Auto starts Downloads
    /// </summary>
    public void Start()
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
        System.Diagnostics.Debug.WriteLine("Start Auto downloads");
        LongTask(CancellationTokenSource.Token);
    }

    /// <summary>
    /// A method that Stops auto downloads
    /// </summary>
    public void Stop()
    {
        if (CancellationTokenSource is not null)
        {
            CancellationTokenSource.Cancel();
            LongTask(CancellationTokenSource.Token);
            CancellationTokenSource?.Dispose();
            CancellationTokenSource = null;
        }
        System.Diagnostics.Debug.WriteLine("Stopped Auto Downloder");
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
        System.Diagnostics.Debug.WriteLine(Status);
        if (Status == string.Empty)
        {
            System.Diagnostics.Debug.WriteLine("No wifi or cell service");
            return;
        }
        if (WifiOnlyDownloading == "Yes" && !Status.Contains("WiFi"))
        {
            System.Diagnostics.Debug.WriteLine("Turning off AutoDownloader. Cellular on connection and Wifi only Downloading turned on");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"Timed event: {e} Started");
        _ = DownloadService.AutoDownload();
    }
}
