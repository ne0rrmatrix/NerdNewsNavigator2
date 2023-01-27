// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application = Microsoft.Maui.Controls.Application;
using Platform = Microsoft.Maui.ApplicationModel.Platform;

#if ANDROID
using View = AndroidX.Core.View;
#endif

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT;
using Microsoft.Maui.Controls;
#endif

namespace NerdNewsNavigator2;
public partial class App : Application
{
    private bool FullScreenMode { get; set; }
    public App()
    {
        FullScreenMode = false;
        InitializeComponent();

        MainPage = new AppShell();
    }
    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = base.CreateWindow(activationState);

        window.Created += (s, e) =>
        {
            FullScreenMode = true; //NOTE: Change this to fetch the value true/false according to your app logic.
            SetFullScreen(s, e);
        };

        window.Resumed += (s, e) =>
        {
            //When resumed, the nav & status bar reappeared for android.
            //Fixing it by calling SetFullScreen again on resume,
            //If fullscreen had been set.
            //Not sure if it is needed for windows. Haven't tested yet.
            if (FullScreenMode)
            {
                SetFullScreen(s, e);
            }
        };

        return window;
    }

    private void SetFullScreen(object? sender, EventArgs eventArgs)
    {
#if ANDROID
        SetFullScreenAndroid();
#endif
#if WINDOWS
        SetFullScreenWindows(sender, eventArgs);
#endif
    }

    private void SetFullScreenAndroid()
    {
    }

    private void SetFullScreenWindows(object? sender, EventArgs eventArgs)
    {
#if WINDOWS
        if(sender is not null)
        {
            var currentWindow = sender.As<Window>();
            var uiWindow = currentWindow.Handler.PlatformView.As<MauiWinUIWindow>();
            var handle = WinRT.Interop.WindowNative.GetWindowHandle(uiWindow);
            var id = Win32Interop.GetWindowIdFromWindow(handle);
            var appWindow = AppWindow.GetFromWindowId(id);
            switch (appWindow.Presenter)
            {
                case OverlappedPresenter overlappedPresenter:
                    //uiWindow.ExtendsContentIntoTitleBar = true;
                    if(FullScreenMode) {
                        overlappedPresenter.SetBorderAndTitleBar(false, false);
                        overlappedPresenter.Maximize();
                    }
                    else
                    {
                        overlappedPresenter.SetBorderAndTitleBar(true, true);
                        overlappedPresenter.Restore();
                    }
                    break;
            }                
        }
#endif
    }
}
