// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using NerdNewsNavigator2.Platforms.Android;

namespace NerdNewsNavigator2;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public static bool SetAutoDownload { get; set; }
    public MainActivity()
    {
        var messenger = MauiApplication.Current.Services.GetService<IMessenger>();
        SetAutoDownload = Preferences.Default.Get("AutoDownload", true);
        System.Diagnostics.Debug.WriteLine($"SetAutoDownload = {SetAutoDownload}");
        messenger.Register<MessageData>(this, (recipient, message) =>
        {
            if (message.Start && SetAutoDownload)
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
    { System.Diagnostics.Debug.WriteLine(e.ToString()); }

    private void StartService()
    {
        System.Diagnostics.Debug.WriteLine("Starting Auto Download Service");
        var serviceIntent = new Intent(this, typeof(AutoStartService));
        this.StartForegroundService(serviceIntent);
    }

    private void StopService()
    {
        System.Diagnostics.Debug.WriteLine("Stopping Auto Download Service");
        var serviceIntent = new Intent(this, typeof(AutoStartService));
        StopService(serviceIntent);
    }
}
