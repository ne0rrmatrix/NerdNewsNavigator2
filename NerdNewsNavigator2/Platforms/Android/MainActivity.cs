// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace NerdNewsNavigator2.Platforms.Android;
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleInstance, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private AutoDownloadService AutoDownloadService { get; set; } = new();
    private static readonly ILogger s_logger = LoggerFactory.GetLogger(nameof(MainActivity));
    public MainActivity()
    {
        var messenger = MauiApplication.Current.Services.GetService<IMessenger>();
        messenger.Register<MessageData>(this, (recipient, message) =>
        {
            if (message.Start)
            {
                StartService();
            }
            else
            {
                StopService();
            }
        });
    }
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        s_logger.Info(e.ToString());
    }

    #region Notification Service Methods
    private void StartService()
    {
        var serviceIntent = new Intent(this, typeof(AutoStartService));
        StartForegroundService(serviceIntent);
    }
    private void StopService()
    {
        if (AutoDownloadService.CancellationTokenSource is not null)
        {
            AutoDownloadService.CancellationTokenSource.Cancel();
            _ = AutoDownloadService.LongTaskAsync(AutoDownloadService.CancellationTokenSource.Token);
            AutoDownloadService.CancellationTokenSource?.Dispose();
            AutoDownloadService.CancellationTokenSource = null;
        }

        s_logger.Info("Stopping AutoDownload");
        var serviceIntent = new Intent(this, typeof(AutoStartService));
        StopService(serviceIntent);
    }
    #endregion
}
