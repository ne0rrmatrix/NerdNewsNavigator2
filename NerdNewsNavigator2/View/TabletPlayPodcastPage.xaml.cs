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
    #region Properties

    public string PlayPosition { get; set; }
    /// <summary>
    /// Initilizes a new instance of the <see cref="ILogger{TCategoryName}"/> class
    /// </summary>
    private readonly ILogger<TabletPlayPodcastPage> _logger;

    /// <summary>
    /// Initilizes a new instance of the <see cref="Position"/> class
    /// </summary>
    private Position Pos { get; set; } = new();

    #endregion

    /// <summary>
    /// Class Constructor that initilizes <see cref="TabletPlayPodcastPage"/>
    /// </summary>
    /// <param name="logger">This Applications <see cref="ILogger{TCategoryName}"/> instance is managed through this class</param>
    /// <param name="viewModel">This Applications <see cref="TabletPlayPodcastPage"/> instance is managed through this class.</param>

    public TabletPlayPodcastPage(ILogger<TabletPlayPodcastPage> logger, TabletPlayPodcastViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _logger = logger;
        PlayPosition = string.Empty;

    }

    /// <summary>
    /// Method overrides <see cref="OnDisappearing"/> to stop playback when leaving a page.
    /// </summary>
    protected override void OnDisappearing()
    {
        mediaElement.Stop();
        mediaElement.ShouldKeepScreenOn = false;
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
        // Stop and cleanup MediaElement when we navigate away
        _logger.LogInformation("Unloaded Media Element from memory.");
        mediaElement.StateChanged -= mediaElement.Media_Stopped;
        Pos.SavedPosition = TimeSpan.Zero;
        Pos.Title = string.Empty;
        mediaElement.Handler?.DisconnectHandler();
    }

#nullable disable

    #endregion
    public void ContentPage_Loaded(object sender, EventArgs e)
    {
#if WINDOWS
        BaseViewModel.CurrentWindow = GetParentWindow().Handler.PlatformView as MauiWinUIWindow;
#endif
        //mediaElement.LoadUrl(Preferences.Default.Get("New_Url", string.Empty));
        mediaElement.Load();
    }
}
