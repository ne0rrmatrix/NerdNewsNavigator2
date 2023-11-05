// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NerdNewsNavigator2.Devices;

namespace NerdNewsNavigator2;
public static class CustomControls
{
    private static readonly DeviceServices s_deviceServices = new();

    private static DeviceServices Control { get; set; } = s_deviceServices;
    private static bool IsFullScreen { get; set; }
    /// <summary>
    /// Toggle Page Full Screen
    /// </summary>
    public static void SetFullScreenStatus()
    {
        if (IsFullScreen)
        {
            IsFullScreen = false;
            Control.RestoreScreen();
        }
        else
        {
            IsFullScreen = true;
            Control.FullScreen();
        }
    }
    public static void FullScreen()
    {
        IsFullScreen = true;
        Control.FullScreen();
    }
    public static void RestoreScreen()
    {
        IsFullScreen = false;
        Control.RestoreScreen();
    }
}
