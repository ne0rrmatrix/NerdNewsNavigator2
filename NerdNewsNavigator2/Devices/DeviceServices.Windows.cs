﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if WINDOWS
using Microsoft.UI.Windowing;
#endif

namespace NerdNewsNavigator2.Devices;
internal sealed class DeviceServices : IDeviceServices
{
    public static Page CurrentPage =>
        PageExtensions.GetCurrentPage(Application.Current?.MainPage ?? throw new InvalidOperationException($"{nameof(Application.Current.MainPage)} cannot be null."));
    public AppWindow FullScreen()
    {
        var currentPage = CurrentPage;
        PageExtensions.SetBarStatus(true);
        var item = currentPage.GetParentWindow().Handler.PlatformView as MauiWinUIWindow;
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

    public AppWindow RestoreScreen()
    {
        var currentPage = CurrentPage;
        PageExtensions.SetBarStatus(false);
        var item = currentPage.GetParentWindow().Handler.PlatformView as MauiWinUIWindow;
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
