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

/// <summary>
/// A class that acts as a manager for <see cref="Application"/>
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// This applications Dependancy Injection for <see cref="PositionDataBase"/> class.
    /// </summary>
    public static PositionDataBase PositionData { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="positionDataBase"></param>
    public App(PositionDataBase positionDataBase)
    {
        InitializeComponent();
        MainPage = new AppShell();

        // Database Dependancy Injection START
        PositionData = positionDataBase;
        // Database Dependancy Injection END

        LogController.InitializeNavigation(
            page => MainPage!.Navigation.PushModalAsync(page),
            () => MainPage!.Navigation.PopModalAsync());
    }

#nullable enable

    /// <summary>
    /// A method to override the default <see cref="Window"/> behavior.
    /// </summary>
    /// <param name="activationState"></param>
    /// <returns></returns>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = base.CreateWindow(activationState);
        window.Created += (s, e) =>
        {
            //NOTE: Change this to fetch the value true/false according to your app logic.
            SetFullScreen(s, e);
        };
        window.Resumed += (s, e) =>
        {
            SetFullScreen(s, e);
        };

        return window;
    }

    /// <summary>
    /// Method to set Full Screen status depending on <see cref="FullScreenMode"/> variable.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    private static void SetFullScreen(object? sender, EventArgs eventArgs)
    {
        if (sender != null)
        {
            Debug.WriteLine("SetFullScreen Triggered");

#if WINDOWS
            var currentWindow = sender.As<Window>();
            var uiWindow = currentWindow.Handler.PlatformView.As<MauiWinUIWindow>();
            var handle = WinRT.Interop.WindowNative.GetWindowHandle(uiWindow);
            var id = Win32Interop.GetWindowIdFromWindow(handle);
            var appWindow = AppWindow.GetFromWindowId(id);
            switch (appWindow.Presenter)
            {
                case OverlappedPresenter overlappedPresenter:
                uiWindow.ExtendsContentIntoTitleBar = false;
                {
                    overlappedPresenter.SetBorderAndTitleBar(true, true);
                    overlappedPresenter.Restore();
                }
                break;
            }
#endif
        }
    }

#nullable disable
}

