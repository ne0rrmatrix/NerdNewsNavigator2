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
        FullScreenMode = Preferences.Default.Get("FullScreen", false);
        SetFullScreen();
    }
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
    }

#nullable disable

}
