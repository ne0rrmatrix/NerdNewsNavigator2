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
    public async Task Start()
    {
        s_logger.Info("Start Auto downloads");
        ATimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
        ATimer.Start();
        if (App.DownloadService.Shows.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine("Manual download in progress. Auto downloader aborting!");
            return;
        }
        var favoriteShows = await App.PositionData.GetAllFavorites();
        if (CheckIfWifiOnly())
        {
            await ProccessShowAsync(favoriteShows);
        }
    }
    /// <summary>
    /// A method that Stops auto downloads
    /// </summary>
    public void Stop()
    {
        App.DownloadService.CancelAll();
        ATimer.Stop();
        ATimer.Elapsed -= new System.Timers.ElapsedEventHandler(OnTimedEvent);
        s_logger.Info("Stopped Auto Downloder");
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
            _ = Start();
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
    private static async Task ProccessShowAsync(List<Favorites> favoriteShows)
    {
        var downloadedShows = await App.PositionData.GetAllDownloads();
        _ = Task.Run(() =>
        {
            if (App.DownloadService.Shows.Count > 0)
            {
                s_logger.Info("Manual dowload in progress. Cancelling auto download");
                return;
            }
            favoriteShows.ForEach(x =>
            {
                var show = FeedService.GetShows(x.Url, true);
                if (show is not null && !downloadedShows.Exists(y => y.Url == show[0].Url))
                {
                    App.DownloadService.Add(show[0]);
                }
            });
            if (App.DownloadService.Shows.Count > 0)
            {
                s_logger.Info("Starting to download favorite shows");
#if ANDROID || IOS
                _ = App.DownloadService.Start(App.DownloadService.Shows[0]);
#else
                App.DownloadService.Start(App.DownloadService.Shows[0]);
#endif
            }
        });
    }
}
