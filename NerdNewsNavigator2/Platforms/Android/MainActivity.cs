// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace NerdNewsNavigator2;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Window.AddFlags(WindowManagerFlags.KeepScreenOn);
        Window.DecorView.SystemUiVisibility = (StatusBarVisibility)
            (SystemUiFlags.ImmersiveSticky | SystemUiFlags.HideNavigation | SystemUiFlags.LowProfile |
            SystemUiFlags.Fullscreen | SystemUiFlags.Immersive);
    }
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    { System.Diagnostics.Debug.WriteLine(e.ToString()); }
}
