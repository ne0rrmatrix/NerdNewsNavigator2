// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that Displays a Video from twit.tv.
/// </summary>
/// 
public partial class VideoPlayerPage : ContentPage
{
    #region Properties
    public ObservableCollection<YoutubeResolutions> Items { get; set; } = new();

    /// <summary>
    /// Initilizes a new instance of the <see cref="ILogger{TCategoryName}"/> class
    /// </summary>
    private readonly ILogger<VideoPlayerPage> _logger;

    /// <summary>
    /// Initilizes a new instance of the <see cref="Position"/> class
    /// </summary>
    private Position Pos { get; set; } = new();

    #endregion
    /// <summary>
    /// Class Constructor that initilizes <see cref="VideoPlayerPage"/>
    /// </summary>
    /// <param name="viewModel">This Applications <see cref="VideoPlayerPage"/> instance is managed through this class.</param>

    public VideoPlayerPage(ILogger<VideoPlayerPage> logger, VideoPlayerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _logger = logger;

#if WINDOWS || ANDROID
        mediaElement.MediaOpened += Seek;
#endif

#if IOS || MACCATALYST
        mediaElement.StateChanged += SeekIOS;
#endif
    }

    #region Events
#nullable enable

    /// <summary>
    /// Manages <see cref="mediaElement"/> seeking of <see cref="Position"/> at start of playback.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Seek(object? sender, EventArgs e)
    {
        Pos.SavedPosition = TimeSpan.Zero;
        Pos.Title = string.Empty;
        var positionList = await App.PositionData.GetAllPositions();
        var result = positionList.FirstOrDefault(x => x.Title == Pos.Title);
        if (result is not null)
        {
            Pos = result;
            _logger.LogInformation("Retrieved Saved position from database is: {Title} - {TotalSeconds}", Pos.Title, Pos.SavedPosition);
            mediaElement.SeekTo(Pos.SavedPosition);
        }
        else
        {
            _logger.LogInformation("Could not find saved position");
        }

        mediaElement.ShouldKeepScreenOn = true;
        _logger.LogInformation("ShouldKeepScreenOn is set to {data}", mediaElement.ShouldKeepScreenOn);
        mediaElement.StateChanged += MediaStopped;
    }

    /// <summary>
    /// Manages IOS seeking for <see cref="mediaElement"/> with <see cref="Pos"/> at start of playback.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SeekIOS(object? sender, MediaStateChangedEventArgs e)
    {
        if (e.NewState == MediaElementState.Opening)
        {
            Pos.Title = string.Empty;
            Pos.SavedPosition = TimeSpan.Zero;
            var positionList = await App.PositionData.GetAllPositions();
            var result = positionList.FirstOrDefault(x => x.Title == Pos.Title);
            if (result is not null)
            {
                Pos = result;
                _logger.LogInformation("Retrieved Saved position from database is: {Title} - {TotalSeconds}", Pos.Title, Pos.SavedPosition);
            }
            else
            {
                _logger.LogInformation("Could not find saved position");
            }
            mediaElement.SeekTo(Pos.SavedPosition);
            mediaElement.ShouldKeepScreenOn = true;
            _logger.LogInformation("Media playback started. ShouldKeepScreenOn is set to true.");
            mediaElement.StateChanged += MediaStopped;
        }
    }

    /// <summary>
    /// Manages the saving of <see cref="Position"/> data in <see cref="PositionDataBase"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MediaStopped(object? sender, MediaStateChangedEventArgs e)
    {
        switch (e.NewState)
        {
            case MediaElementState.Stopped:
                _logger.LogInformation("Media has finished playing.");
                mediaElement.ShouldKeepScreenOn = false;
                _logger.LogInformation("ShouldKeepScreenOn set to false.");
                break;

            case MediaElementState.Paused:
                if (mediaElement.Position > Pos.SavedPosition)
                {
                    Pos.SavedPosition = mediaElement.Position;
                    _logger.LogInformation("Paused: {Position}", mediaElement.Position);
                    await App.PositionData.UpdatePosition(Pos);
                }
                break;
        }
        switch (e.PreviousState)
        {
            case MediaElementState.Playing:
                if (mediaElement.Position < Pos.SavedPosition)
                {
                    Pos.SavedPosition = mediaElement.Position;
                    _logger.LogInformation("Finished Seeking: {Position}", mediaElement.Position);
                    await App.PositionData.UpdatePosition(Pos);
                }
                break;
        }
    }

#nullable disable
    private void ContentPage_Unloaded(object sender, EventArgs e)
    {
        if (mediaElement is not null)
        {
            _logger.LogInformation("Page unloaded. Media playback Stopped. ShouldKeepScreenOn is set to {data}", mediaElement.ShouldKeepScreenOn);
            mediaElement.Handler?.DisconnectHandler();
        }
    }

    #endregion

    /// <summary>
    /// Method overrides <see cref="OnDisappearing"/> to stop playback when leaving a page.
    /// </summary>
    protected override void OnDisappearing()
    {
        if (mediaElement is not null)
        {
            mediaElement.Stop();
            _logger.LogInformation("Page dissapearing. Media playback Stopped. ShouldKeepScreenOn is set to {data}", mediaElement.ShouldKeepScreenOn);
        }
    }
}
