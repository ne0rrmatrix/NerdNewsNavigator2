﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Platform = Microsoft.Maui.ApplicationModel.Platform;

#if ANDROID
using Views = AndroidX.Core.View;
#endif

namespace NerdNewsNavigator2.Services;
internal static partial class DeviceService
{
    #region Full Screen Methods
    public static partial void RestoreScreen()
    {
        Shell.Current.CurrentPage.Title = string.Empty;
        Shell.SetTabBarIsVisible(Shell.Current.CurrentPage, true);
        Shell.SetNavBarIsVisible(Shell.Current.CurrentPage, true);
        var activity = Platform.CurrentActivity;

        if (activity == null || activity.Window == null)
        {
            return;
        }

        Views.WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);
        var windowInsetsControllerCompat = Views.WindowCompat.GetInsetsController(activity.Window, activity.Window.DecorView);
        var types = Views.WindowInsetsCompat.Type.StatusBars() |
                    Views.WindowInsetsCompat.Type.NavigationBars();
        windowInsetsControllerCompat.Show(types);
    }
    public static partial void FullScreen()
    {
        Shell.SetTabBarIsVisible(Shell.Current.CurrentPage, false);
        Shell.SetNavBarIsVisible(Shell.Current.CurrentPage, false);
        var activity = Platform.CurrentActivity;

        if (activity == null || activity.Window == null)
        {
            return;
        }

        Views.WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);
        var windowInsetsControllerCompat = Views.WindowCompat.GetInsetsController(activity.Window, activity.Window.DecorView);
        var types = Views.WindowInsetsCompat.Type.StatusBars() |
                    Views.WindowInsetsCompat.Type.NavigationBars();

        windowInsetsControllerCompat.SystemBarsBehavior = Views.WindowInsetsControllerCompat.BehaviorShowBarsBySwipe;
        windowInsetsControllerCompat.Hide(types);
    }
    #endregion
}
