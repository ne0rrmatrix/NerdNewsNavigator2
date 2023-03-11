// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application = Microsoft.Maui.Controls.Application;
using Platform = Microsoft.Maui.ApplicationModel.Platform;
using MetroLog.Maui;

#if ANDROID
using Views = AndroidX.Core.View;
#endif

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that displays a <see cref="List{T}"/> of <see cref="Podcast"/> from twit.tv network.
/// </summary>
public partial class TabletPodcastPage : ContentPage
{
    /// <summary>
    /// Private <see cref="bool"/> which sets Full Screen Mode.
    /// </summary>
    private bool FullScreenMode { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TabletPodcastPage"/> class.
    /// </summary>
    /// <param name="viewModel">This pages <see cref="ViewModel"/> from <see cref="TabletPodcastViewModel"/></param>
    public TabletPodcastPage(TabletPodcastViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        FullScreenMode = false;
    }

#if WINDOWS
    /// <summary>
    /// Method is required for switching Full Screen Mode for Windows
    /// </summary>
    private static Microsoft.UI.Windowing.AppWindow GetAppWindow(MauiWinUIWindow window)
    {
        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
        return appWindow;
    }
#endif

    /// <summary>
    /// Method toggles Full Screen On/Off
    /// </summary>

#nullable enable
    private void SetFullScreen()
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
#if WINDOWS
        var window = GetParentWindow().Handler.PlatformView as MauiWinUIWindow;
        if (window is not null)
        {
            var appWindow = GetAppWindow(window);

            switch (appWindow.Presenter)
            {
                case Microsoft.UI.Windowing.OverlappedPresenter overlappedPresenter:
                    if (overlappedPresenter.State == Microsoft.UI.Windowing.OverlappedPresenterState.Maximized)
                    {
                        overlappedPresenter.SetBorderAndTitleBar(true, true);
                        overlappedPresenter.Restore();
                    }
                    break;
            }
        }
#endif
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        SetFullScreen();
    }

#nullable disable

}
