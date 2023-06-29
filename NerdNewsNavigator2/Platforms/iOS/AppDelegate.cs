// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BackgroundTasks;
using Foundation;
using SQLitePCL;
using UIKit;

namespace NerdNewsNavigator2.Platforms.iOS;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    private bool IsRunning { get; set; }

    private IConnectivity _connectivity;
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
        AutoDownload();
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
        Console.WriteLine("App will enter foreground");
        AutoDownload();
        base.WillEnterForeground(application);
    }
    public AppDelegate() : base()
    {
    }
    public void AutoDownload()
    {
        IsRunning = Preferences.Default.Get("AutoDownload", true);
        if (InternetConnected() && IsRunning)
        {
            ThreadPool.QueueUserWorkItem(async state =>
        {
            await DownloadService.AutoDownload();
        });
        }
    }
}
