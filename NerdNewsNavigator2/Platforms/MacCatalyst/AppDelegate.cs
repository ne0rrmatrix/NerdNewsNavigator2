// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BackgroundTasks;
using Foundation;
using SQLitePCL;
using UIKit;
namespace NerdNewsNavigator2.Platforms.MacCatalyst;

[Register("AppDelegate")]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class AppDelegate : MauiUIApplicationDelegate
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    private bool IsRunning { get; set; }

    private IConnectivity _connectivity;
    private static ILogger Logger { get; set; }
    private AutoDownloadService AutoDownloadService { get; set; }
    public static string DownloadTaskId { get; } = "com.yourappname.upload";
    public static string RefreshTaskId { get; } = "com.yourappname.refresh";
    // Next line is for SqlLite
    protected override MauiApp CreateMauiApp()
    {
        raw.SetProvider(new SQLite3Provider_sqlite3());
        BGTaskScheduler.Shared.Register(DownloadTaskId, null, task => HandleDownload(task as BGProcessingTask));
        BGTaskScheduler.Shared.Register(RefreshTaskId, null, task => HandleAppRefresh(task as BGAppRefreshTask));
        return MauiProgram.CreateMauiApp();
    }
    public override void OnActivated(UIApplication application)
    {
        base.OnActivated(application);
        Logger = LoggerFactory.GetLogger(nameof(AppDelegate));
        AutoDownloadService = new AutoDownloadService();
        _connectivity = Current.Services.GetService<IConnectivity>();
    }

    /// <summary>
    /// A method that checks if the internet is connected and returns a <see cref="bool"/> as answer.
    /// </summary>
    /// <returns></returns>
    public bool InternetConnected()
    {
        return _connectivity.NetworkAccess == NetworkAccess.Internet;
    }
    private void HandleDownload(BGTask task)
    {
        AutoDownloadAsync();
        task.SetTaskCompleted(true);
    }
    private void HandleAppRefresh(BGAppRefreshTask task)
    {
        task.ExpirationHandler = () =>
        {
            var refresh = new BGAppRefreshTaskRequest(RefreshTaskId);
            BGTaskScheduler.Shared.Submit(refresh, out var refreshError);

            if (refreshError != null)
            {
                Debug.WriteLine(refreshError);
            }
        };
        HandleDownload(task);
    }
    public override void WillEnterForeground(UIApplication application)
    {
        Logger.Info("App will enter foreground");
        AutoDownloadAsync();
        base.WillEnterForeground(application);
    }
    public AppDelegate() : base()
    {
    }
    public void AutoDownloadAsync()
    {
        IsRunning = Preferences.Default.Get("AutoDownload", true);
        if (InternetConnected() && IsRunning)
        {
            AutoDownloadService.Start();
        }
    }
}
