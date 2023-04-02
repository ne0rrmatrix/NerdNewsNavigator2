﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.UI.Windowing;

namespace NerdNewsNavigator2.Service;
internal static partial class DeviceService
{
    public static Page CurrentPage { get; private set; }
    public static partial AppWindow FullScreen()
    {
        CurrentPage = Shell.Current.CurrentPage;
        var item = CurrentPage.GetParentWindow().Handler.PlatformView as MauiWinUIWindow;
        var currentWindow = GetAppWindow(item);
        switch (currentWindow.Presenter)
        {
            case OverlappedPresenter overlappedPresenter:
                overlappedPresenter.SetBorderAndTitleBar(false, false);
                overlappedPresenter.Maximize();
                break;
        }
        return currentWindow;
    }
    public static partial AppWindow RestoreScreen()
    {
        CurrentPage = Shell.Current.CurrentPage;
        var item = CurrentPage.GetParentWindow().Handler.PlatformView as MauiWinUIWindow;
        var currentWindow = GetAppWindow(item);
        switch (currentWindow.Presenter)
        {
            case OverlappedPresenter overlappedPresenter:
                if (overlappedPresenter.State == Microsoft.UI.Windowing.OverlappedPresenterState.Maximized)
                {
                    overlappedPresenter.SetBorderAndTitleBar(true, true);
                    overlappedPresenter.Restore();
                }
                break;
        }
        return currentWindow;
    }
    private static AppWindow GetAppWindow(MauiWinUIWindow window)
    {
        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
        var appWindows = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
        return appWindows;
    }
}