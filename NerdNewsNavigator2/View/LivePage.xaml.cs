// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Maui.Controls.PlatformConfiguration;
using NerdNewsNavigator2.controls;
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

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that manages watching Live video from twit.tv podcasting network
/// </summary>
public partial class LivePage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of <see cref="LivePage"/> class.
    /// </summary>
    /// <param name="liveViewModel">This classes <see cref="ViewModel"/> from <see cref="LiveViewModel"/></param>
    public LivePage(LiveViewModel liveViewModel)
    {
        InitializeComponent();
        BindingContext = liveViewModel;
    }

    /// <summary>
    /// Method overrides <see cref="OnDisappearing"/> to stop playback when leaving a page.
    /// </summary>
    protected override void OnDisappearing()
    {
        mediaElement.ShouldKeepScreenOn = false;
        mediaElement.Stop();
    }
    #region Load/Unload Events
#nullable enable

    /// <summary>
    /// Manages unload event from <see cref="mediaElement"/> after it is unloaded.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void ContentPage_Unloaded(object? sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        mediaElement.Handler?.DisconnectHandler();
    }

    /// <summary>
    /// Method Loads Video after page has finished being rendered.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContentPage_Loaded(object? sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }

#if WINDOWS
        BaseViewModel.CurrentWindow = GetParentWindow().Handler.PlatformView as MauiWinUIWindow;
#endif
        _ = mediaElement.LoadVideo();
    }

#nullable disable

    #endregion
}
