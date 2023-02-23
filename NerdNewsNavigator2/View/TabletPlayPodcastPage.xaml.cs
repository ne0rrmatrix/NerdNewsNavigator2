﻿// Licensed to the .NET Foundation under one or more agreements.
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

    /// <summary>
    /// Initilizes a new instance of the <see cref="ILogger{TCategoryName}"/> class
    /// </summary>
    private readonly ILogger<TabletPlayPodcastPage> _logger;

    /// <summary>
    /// Initilizes a new instance of the <see cref="Position"/> class
    /// </summary>
    private Position Pos { get; set; } = new();

    /// <summary>
    /// Instance variable for tracking whether a video has started. <see cref="MediaStateChangedEventArgs"/>
    /// </summary>
    private bool _isPlaying = false;

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

#if WINDOWS || ANDROID
        mediaElement.MediaOpened += Seek;
#endif
#if IOS
        mediaElement.StateChanged += SeekIOS;
#endif
    }
#nullable enable

    /// <summary>
    /// Manages the saving of <see cref="Position"/> data in <see cref="PositionDataBase"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public async void Media_Stopped(object? sender, MediaStateChangedEventArgs e)
    {
        if (sender is null || _isPlaying == false)
        {
            return;
        }
        if ((mediaElement.CurrentState == MediaElementState.Paused) && mediaElement.Position != Pos.SavedPosition)
        {
            Pos.SavedPosition = mediaElement.Position;
            _logger.LogInformation("Paused: {Position}", mediaElement.Position);
            await Save();
        }
    }

    /// <summary>
    /// Manages <see cref="mediaElement"/> seeking of <see cref="Position"/> at start of playback.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public async void Seek(object? sender, EventArgs e)
    {
        if (sender is null && _isPlaying == false)
        {
            return;
        }
        Pos.Title = Preferences.Default.Get("New_Url", string.Empty);
        Pos.SavedPosition = TimeSpan.Zero;
        var positionList = await App.PositionData.GetAllPositions();
        foreach (var item in positionList)
        {
            if (Pos.Title == item.Title)
            {
                Pos.SavedPosition = item.SavedPosition;
                _logger.LogInformation("Retrieved Saved position from database is: {Title} - {TotalSeconds}", item.Title, item.SavedPosition.TotalSeconds);
            }
        }
        mediaElement.SeekTo(Pos.SavedPosition);
        _isPlaying = true;
        mediaElement.StateChanged += Media_Stopped;
    }

    /// <summary>
    /// Manages IOS seeking for <see cref="mediaElement"/> with <see cref="Pos"/> at start of playback.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public async void SeekIOS(object sender, MediaStateChangedEventArgs e)
    {
        Pos.Title = Preferences.Default.Get("New_Url", string.Empty);
        Pos.SavedPosition = TimeSpan.Zero;
        var positionList = await App.PositionData.GetAllPositions();
        foreach (var item in positionList)
        {
            if (Pos.Title == item.Title)
            {
                Pos.SavedPosition = item.SavedPosition;
                _logger.LogInformation("Retrieved Saved position from database is: {Title} - {TotalSeconds}", item.Title, item.SavedPosition.TotalSeconds);
            }
        }
        if (e.NewState == MediaElementState.Playing)
        {
            mediaElement.SeekTo(Pos.SavedPosition);
        }
        _isPlaying = true;
        mediaElement.StateChanged += Media_Stopped;
    }

    /// <summary>
    /// Manages saving of <see cref="Pos"/> to <see cref="PositionDataBase"/> Database.
    /// </summary>
    /// <returns></returns>
    private async Task Save()
    {
        await App.PositionData.Add(new Position
        {
            Title = Pos.Title,
            SavedPosition = Pos.SavedPosition,
        });
    }

    /// <summary>
    /// Manages unload event from <see cref="mediaElement"/> after it is unloaded.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContentPage_Unloaded(object? sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        // Stop and cleanup MediaElement when we navigate away
        _logger.LogInformation("Unloaded Media Element from memory.");
        mediaElement.MediaOpened -= Seek;
        mediaElement.StateChanged -= Media_Stopped;
        mediaElement.Handler?.DisconnectHandler();
    }
}
