// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if ANDROID
using Android.OS;
using static Android.OS.PowerManager;
#endif

namespace NerdNewsNavigator2.Services;
public partial class AutoDownloadService
{
    private string Status { get; set; }
    private string WifiOnlyDownloading { get; set; }
    private System.Timers.Timer ATimer { get; set; } = new(60 * 60 * 1000);
    public CancellationTokenSource CancellationTokenSource { get; set; } = null;
    private static readonly ILogger s_logger = LoggerFactory.GetLogger(nameof(AutoDownloadService));
    public AutoDownloadService()
    {
        Status = string.Join(", ", Connectivity.Current.ConnectionProfiles);
        WifiOnlyDownloading = Preferences.Default.Get("WifiOnly", "No");
        Connectivity.Current.ConnectivityChanged += GetCurrentConnectivity;
    }

    /// <summary>
    /// A method that Auto starts Downloads
    /// </summary>
    public void Start()
    {
        if (CancellationTokenSource is not null)
        {
            CancellationTokenSource.Dispose();
            CancellationTokenSource = null;
            var cts = new CancellationTokenSource();
            CancellationTokenSource = cts;
        }
        else if (CancellationTokenSource is null)
        {
            var cts = new CancellationTokenSource();
            CancellationTokenSource = cts;
        }
        s_logger.Info("Start Auto downloads");
        _ = LongTaskAsync(CancellationTokenSource.Token);
    }

    /// <summary>
    /// A method that Stops auto downloads
    /// </summary>
    public void Stop()
    {
        if (CancellationTokenSource is not null)
        {
            CancellationTokenSource.Cancel();
            App.DownloadService.CancellationTokenSource.Cancel();
            _ = LongTaskAsync(CancellationTokenSource.Token);
            CancellationTokenSource?.Dispose();
            CancellationTokenSource = null;
        }
        s_logger.Info("Stopped Auto Downloder");
    }
    public async Task LongTaskAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            ATimer.Stop();
            App.DownloadService.CancellationTokenSource?.Cancel();
            ATimer.Elapsed -= new System.Timers.ElapsedEventHandler(OnTimedEvent);
            return;
        }
        ATimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
        ATimer.Start();
        if (CheckIfWifiOnly())
        {
            await App.DownloadService.AutoDownload();
        }
    }
    private void GetCurrentConnectivity(object sender, ConnectivityChangedEventArgs e)
    {
        s_logger.Info("Connection status has changed");
        Status = string.Join(", ", Connectivity.Current.ConnectionProfiles);
        s_logger.Info(Status);
        WifiOnlyDownloading = Preferences.Default.Get("WifiOnly", "No");
    }
    private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
    {
        if (CheckIfWifiOnly())
        {
            s_logger.Info($"Timed event: {e} Started");
            _ = App.DownloadService.AutoDownload();
            return;
        }
        s_logger.Info("Auto Downloader not started");
    }

    public bool CheckIfWifiOnly()
    {
        WifiOnlyDownloading = Preferences.Default.Get("WifiOnly", "No");
        s_logger.Info(Status);
        if (Status == string.Empty)
        {
            s_logger.Info("No wifi or cell service");
            return false;
        }
        if (WifiOnlyDownloading == "Yes" && !Status.Contains("WiFi"))
        {
            s_logger.Info("Turning off AutoDownloader. Cellular on connection and Wifi only Downloading turned on");
            return false;
        }
        return true;
    }
}
