// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Platform = Microsoft.Maui.ApplicationModel.Platform;

#if ANDROID
using Views = AndroidX.Core.View;
#endif

namespace NerdNewsNavigator2.Devices;

internal class DeviceServices : IDeviceServices
{
    public void RestoreScreen()
    {
        PageExtensions.SetBarStatus(false);
        var activity = Platform.CurrentActivity;

        if (activity == null || activity.Window == null)
        {
            return;
        }

        Views.WindowCompat.SetDecorFitsSystemWindows(activity.Window, true);
        var windowInsetsControllerCompat = Views.WindowCompat.GetInsetsController(activity.Window, activity.Window.DecorView);
        var types = Views.WindowInsetsCompat.Type.StatusBars() |
                    Views.WindowInsetsCompat.Type.NavigationBars();
        windowInsetsControllerCompat.Show(types);
        windowInsetsControllerCompat.SystemBarsBehavior = Views.WindowInsetsControllerCompat.BehaviorDefault;
    }
    public void FullScreen()
    {
        PageExtensions.SetBarStatus(true);
        var activity = Platform.CurrentActivity;

        if (activity == null || activity.Window == null)
        {
            return;
        }

        Views.WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);
        var windowInsetsControllerCompat = Views.WindowCompat.GetInsetsController(activity.Window, activity.Window.DecorView);
        var types = Views.WindowInsetsCompat.Type.StatusBars() |
                    Views.WindowInsetsCompat.Type.NavigationBars();

        windowInsetsControllerCompat.SystemBarsBehavior = Views.WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
        windowInsetsControllerCompat.Hide(types);
    }
}
