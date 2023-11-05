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
    private string WifeOnlyDownloading { get; set; }
    private System.Timers.Timer ATimer { get; set; } = new(60 * 60 * 1000);
    private static readonly ILogger s_logger = LoggerFactory.GetLogger(nameof(AutoDownloadService));
    public AutoDownloadService()
    {
        Status = string.Join(", ", Connectivity.Current.ConnectionProfiles);
        WifeOnlyDownloading = Preferences.Default.Get("WIFIOnly", "No");
        Connectivity.Current.ConnectivityChanged += GetCurrentConnectivity;
    }

    /// <summary>
    /// A method that Auto starts Downloads
    /// </summary>
    public void Start()
    {
        ATimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
        ATimer.Start();
        if (!InternetOk())
        {
            return;
        }
        ThreadPool.QueueUserWorkItem(state => _ = ProcessShowAsync());
    }
    /// <summary>
    /// A method that Stops auto downloads
    /// </summary>
    public void Stop()
    {
        App.DownloadService.CancelAll();
        ATimer.Stop();
        ATimer.Elapsed -= new System.Timers.ElapsedEventHandler(OnTimedEvent);
        s_logger.Info("Stopped Auto Downloader");
    }
    private void GetCurrentConnectivity(object sender, ConnectivityChangedEventArgs e)
    {
        s_logger.Info("Connection status has changed");
        Status = string.Join(", ", Connectivity.Current.ConnectionProfiles);
        s_logger.Info(Status);
        WifeOnlyDownloading = Preferences.Default.Get("WIFIOnly", "No");
    }
    private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
    {
        s_logger.Info($"Timed event: {e} Started");
        Start();
    }

    private bool InternetOk()
    {
        WifeOnlyDownloading = Preferences.Default.Get("WIFIOnly", "No");
        s_logger.Info(Status);
        if (Status.Contains("WiFi"))
        {
            return true;
        }
        if (WifeOnlyDownloading == "No" && Status != string.Empty)
        {
            return true;
        }
        s_logger.Info("No Internet. Aborting Auto downloads!");
        return false;
    }
    private static async Task ProcessShowAsync()
    {
        var downloadedShows = await App.PositionData.GetAllDownloads();
        var favoriteShows = await App.PositionData.GetAllFavorites();

        favoriteShows.ForEach(x =>
        {
            var show = FeedService.GetShows(x.Url, true);
            if (show.Count == 1 && !downloadedShows.Exists(y => y.Url == show[0].Url))
            {
                App.DownloadService.Add(show[0]);
            }
        });

        if (App.DownloadService.Shows.Count == 0)
        {
            s_logger.Info("Nothing to download. Auto Downloader aborting!");
            return;
        }
        ThreadPool.QueueUserWorkItem(state => _ = App.DownloadService.Start(App.DownloadService.Shows[0]));
    }
}
