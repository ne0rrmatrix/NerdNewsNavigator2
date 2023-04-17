// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BackgroundTasks;
using Foundation;
using Microsoft.Maui.Handlers;
using SQLitePCL;
using UIKit;

namespace NerdNewsNavigator2;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
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
    private void HandleDownload(BGTask task)
    {
        _ = NerdNewsNavigator2.App.AutoDownload();
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
    public override void DidEnterBackground(UIApplication application)
    {
        Console.WriteLine("App entering background state.");
    }

    public override void WillEnterForeground(UIApplication application)
    {
        Console.WriteLine("App will enter foreground");
    }
    public AppDelegate() : base()
    {
    }
}
