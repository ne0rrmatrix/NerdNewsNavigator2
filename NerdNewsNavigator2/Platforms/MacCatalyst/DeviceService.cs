// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CoreGraphics;
using System.Runtime.InteropServices;
using ObjCRuntime;
using UIKit;
using Foundation;
using NerdNewsNavigator2.Platforms.MacCatalyst;

namespace NerdNewsNavigator2.Services;
internal static partial class DeviceService
{
    static readonly UIKit.UIWindow s_mainWindow = MauiUIApplicationDelegate.Current.Window;
    [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "IntPtr_objc_msgSend")]
    private static extern nint IntPtr_objc_msgSend(
        ObjCRuntime.NativeHandle handle,
        ObjCRuntime.NativeHandle handle1
        );
    [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "void_objc_msgSend_IntPtr")]
    static extern nint void_objc_msgSend_IntPtr
    (
        ObjCRuntime.NativeHandle handle,
        ObjCRuntime.NativeHandle handle1,
        IntPtr handle2
    );
    [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    static extern CGSize cgsize_objc_msgSend_IntPtr_float_int(
    IntPtr target,
    IntPtr selector,
    IntPtr font,
    nfloat width,
    UILineBreakMode mode
);
    static void ToggleFullScreen()
    {
        if (s_mainWindow is null)
            return;

        var nsApplication = Runtime.GetNSObject(Class.GetHandle("NSApplication"));
        if (nsApplication is null)
            return;

        var sharedApplication = nsApplication.PerformSelector(new Selector("sharedApplication"));
        if (sharedApplication is null)
            return;

        var delegeteSelector = new Selector("delegate");
        if (!sharedApplication.RespondsToSelector(delegeteSelector))
            return;

        var delegeteIntptr = IntPtr_objc_msgSend(sharedApplication.Handle, delegeteSelector.Handle);
        var delegateObject = Runtime.GetNSObject(delegeteIntptr);

        if (delegateObject is null)
            return;

        var hostWindowForUIWindowSelector = new Selector("_hostWindowForUIWindow:");
        if (!delegateObject.RespondsToSelector(hostWindowForUIWindowSelector))
            return;

        var mainWindow = delegateObject.PerformSelector(hostWindowForUIWindowSelector, s_mainWindow.Self);
        if (mainWindow is null)
            return;

        var toggleFullScreenSelector = new Selector("toggleFullScreen:");
        if (!mainWindow.RespondsToSelector(toggleFullScreenSelector))
            return;

        void_objc_msgSend_IntPtr(mainWindow.Handle, toggleFullScreenSelector.Handle, IntPtr.Zero);
    }
    public static partial void FullScreen()
    {
        ToggleFullScreen();

    }
    public static partial void RestoreScreen()
    {
        ToggleFullScreen();
    }
}
