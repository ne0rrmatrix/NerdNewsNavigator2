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
    public static bool IsYoutube { get; set; } = false;
    public static string Url { get; set; } = string.Empty;
    public ObservableCollection<YoutubeResolutions> Items { get; set; } = new();
    /// <summary>
    /// Initilizes a new instance of the <see cref="ILogger{TCategoryName}"/> class
    /// </summary>
    private readonly ILogger<VideoPlayerPage> _logger;
    /// <summary>
    /// Initilizes a new instance of the <see cref="Position"/> class
    /// </summary>
    private Position Pos { get; set; } = new();

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
#nullable enable
    /// <summary>
    /// Manages IOS seeking for <see cref="mediaElement"/> with <see cref="Pos"/> at start of playback.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SeekIOS(object? sender, MediaStateChangedEventArgs e)
    {
        Pos.Title = Url;
        Pos.SavedPosition = TimeSpan.Zero;
        var positionList = await App.PositionData.GetAllPositions();
        foreach (var item in positionList)
        {
            if (Pos.Title == item.Title)
            {
                Pos.SavedPosition = item.SavedPosition;
                _logger.LogInformation("Retrieved Saved position from database is: {Title} - {TotalSeconds}", item.Title, item.SavedPosition);
            }
        }
        if (e.NewState == MediaElementState.Opening)
        {
            mediaElement.SeekTo(Pos.SavedPosition);
            mediaElement.ShouldKeepScreenOn = true;
            _logger.LogInformation("Media playback started. ShouldKeepScreenOn is set to true.");
            mediaElement.StateChanged += Media_Stopped;
        }
    }

    /// <summary>
    /// Manages the saving of <see cref="Position"/> data in <see cref="PositionDataBase"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Media_Stopped(object? sender, MediaStateChangedEventArgs e)
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
                    await Save();
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
                    await Save();
                }
                break;
        }
    }
#nullable disable

    /// <summary>
    /// Manages saving of <see cref="Pos"/> to <see cref="PositionDataBase"/> Database.
    /// </summary>
    /// <returns></returns>
    private async Task Save()
    {
        var items = await App.PositionData.GetAllPositions();
        foreach (var item in items)
        {
            if (item.Title == Pos.Title)
            {
                await App.PositionData.Delete(item);
            }
        }
        await App.PositionData.Add(new Position
        {
            Title = Pos.Title,
            SavedPosition = Pos.SavedPosition,
        });
    }

#nullable enable

    /// <summary>
    /// Manages <see cref="mediaElement"/> seeking of <see cref="Position"/> at start of playback.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Seek(object? sender, EventArgs e)
    {
        Pos.SavedPosition = TimeSpan.Zero;
        Pos.Title = Url;
        var positionList = await App.PositionData.GetAllPositions();
        foreach (var item in positionList)
        {
            if (Pos.Title == item.Title)
            {
                Pos.SavedPosition = item.SavedPosition;
                _logger.LogInformation("Retrieved Saved position from database is: {Title} - {TotalSeconds}", item.Title, item.SavedPosition);
            }
        }
        mediaElement.ShouldKeepScreenOn = true;
        mediaElement.SeekTo(Pos.SavedPosition);
        _logger.LogInformation("Media playback started. ShouldKeepScreenOn is set to {data}", mediaElement.ShouldKeepScreenOn);
        mediaElement.StateChanged += Media_Stopped;
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
}
