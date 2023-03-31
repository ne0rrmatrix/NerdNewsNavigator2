// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that Displays a Video from twit.tv.
/// </summary>
/// 
public partial class TabletPlayPodcastPage : ContentPage
{

    /// <summary>
    /// Class Constructor that initilizes <see cref="TabletPlayPodcastPage"/>
    /// </summary>
    /// <param name="viewModel">This Applications <see cref="TabletPlayPodcastPage"/> instance is managed through this class.</param>

    public TabletPlayPodcastPage(TabletPlayPodcastViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>
    /// Method overrides <see cref="OnDisappearing"/> to stop playback when leaving a page.
    /// </summary>
    protected override void OnDisappearing()
    {
        mediaElement.Stop();
        mediaElement.ShouldKeepScreenOn = false;
    }
    public void ContentPage_Loaded(object sender, EventArgs e)
    {
#if WINDOWS
        BaseViewModel.CurrentWindow = GetParentWindow().Handler.PlatformView as MauiWinUIWindow;
#endif
        mediaElement.Load();

    }
}
