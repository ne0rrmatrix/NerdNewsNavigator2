// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application = Microsoft.Maui.Controls.Application;
using Platform = Microsoft.Maui.ApplicationModel.Platform;

#if ANDROID
using Views = AndroidX.Core.View;
#endif

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT;
using Microsoft.Maui.Controls;
#endif

using MetroLog.Maui;

namespace NerdNewsNavigator2;
public partial class App : Application
{
    private static bool FullScreenMode { get; set; }
    // DataBase Dependancy Injection START
    public static PositionDataBase PositionData { get; private set; }
    // DataBase Dependancy Injection END
    public App(PositionDataBase positionDataBase)
    {
        InitializeComponent();
        MainPage = new AppShell();
        FullScreenMode = false;
        LogController.InitializeNavigation(
            page => MainPage!.Navigation.PushModalAsync(page),
            () => MainPage!.Navigation.PopModalAsync());
        // Database Dependancy Injection START
        PositionData = positionDataBase;
        // Database Dependancy Injection END
    }
#nullable enable
    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = base.CreateWindow(activationState);
        window.Created += (s, e) =>
        {
            FullScreenMode = Preferences.Default.Get("FullScreen", false);
            //NOTE: Change this to fetch the value true/false according to your app logic.
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

    private static void SetFullScreen(object? sender, EventArgs eventArgs)
    {
        if (sender != null)
        {
            Debug.WriteLine("SetFullScreen Triggered");
#if ANDROID
            SetFullScreenAndroid();
#endif
#if WINDOWS
        SetFullScreenWindows(sender, eventArgs);
#endif
        }

    }
#nullable disable
    private static void SetFullScreenAndroid()
    {
#if ANDROID
        var activity = Platform.CurrentActivity;

        if (activity == null || activity.Window == null) return;

        Views.WindowCompat.SetDecorFitsSystemWindows(activity.Window, !FullScreenMode);
        var windowInsetsControllerCompat = Views.WindowCompat.GetInsetsController(activity.Window, activity.Window.DecorView);
        var types = Views.WindowInsetsCompat.Type.StatusBars() |
                    Views.WindowInsetsCompat.Type.NavigationBars();


        if (FullScreenMode)
        {
            windowInsetsControllerCompat.SystemBarsBehavior = Views.WindowInsetsControllerCompat.BehaviorShowBarsBySwipe;
            windowInsetsControllerCompat.Hide(types);
        }
        else
        {
            windowInsetsControllerCompat.Show(types);
        }
#endif
    }
#nullable enable
    private static void SetFullScreenWindows(object? sender, EventArgs eventArgs)
    {
#if WINDOWS
        if (sender is not null)
        {
            var currentWindow = sender.As<Window>();
            var uiWindow = currentWindow.Handler.PlatformView.As<MauiWinUIWindow>();
            var handle = WinRT.Interop.WindowNative.GetWindowHandle(uiWindow);
            var id = Win32Interop.GetWindowIdFromWindow(handle);
            var appWindow = AppWindow.GetFromWindowId(id);
            switch (appWindow.Presenter)
            {
                case OverlappedPresenter overlappedPresenter:
                    uiWindow.ExtendsContentIntoTitleBar = false;
                    if (FullScreenMode)
                    {
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
#nullable disable
}
