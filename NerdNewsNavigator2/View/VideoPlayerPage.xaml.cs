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

    /// <summary>
    /// Initilizes a new instance of the <see cref="ILogger"/> class
    /// </summary>
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(VideoPlayerPage));

    /// <summary>
    /// Initilizes a new instance of the <see cref="Position"/> class
    /// </summary>
    private Position Pos { get; set; } = new();
    private Show ShowItem { get; set; } = new();

    #endregion
    /// <summary>
    /// Class Constructor that initilizes <see cref="VideoPlayerPage"/>
    /// </summary>
    /// <param name="viewModel">This Applications <see cref="VideoPlayerPage"/> instance is managed through this class.</param>

    public VideoPlayerPage(VideoPlayerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        App.OnVideoNavigated.Navigation += Now;
    }

    private async void Now(object sender, VideoNavigationEventArgs e)
    {
        _logger.Info($"Navigated: {e.CurrentShow.Url}");
        App.OnVideoNavigated.Navigation -= Now;
        ShowItem = e.CurrentShow;
        mediaElement.Source = new Uri(e.CurrentShow.Url);
        await Seek(ShowItem);
    }

    #region Events
#nullable enable
    /// <summary>
    /// Manages IOS seeking for <see cref="mediaElement"/> with <see cref="Pos"/> at start of playback.
    /// </summary>
    /// <param name="show"></param>
    private async Task Seek(Show show)
    {
        Pos.SavedPosition = TimeSpan.Zero;
        Pos.Title = show.Title;
        _logger.Info("Title: {Title}", show.Title);
        var positionList = await App.PositionData.GetAllPositions();
        var result = positionList.ToList().Find(x => x.Title == show.Title);
        if (result is not null)
        {
            Pos = result;
            Pos.Title = result.Title;
            Pos.SavedPosition = result.SavedPosition;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                mediaElement.Pause();
                _logger.Info("Retrieved Saved position from database is: {Title} - {TotalSeconds}", Pos.Title, Pos.SavedPosition);
                mediaElement.SeekTo(Pos.SavedPosition);
                mediaElement.Play();
            });
        }
        else
        {
            _logger.Info("Could not find saved position");
        }

        mediaElement.ShouldKeepScreenOn = true;
        mediaElement.StateChanged += MediaStopped;
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
                _logger.Info("Media has finished playing.");
                mediaElement.ShouldKeepScreenOn = false;
                _logger.Info("ShouldKeepScreenOn set to false.");
                break;
            case MediaElementState.Paused:
                if (mediaElement.Position > Pos.SavedPosition)
                {
                    Pos.SavedPosition = mediaElement.Position;
                    _logger.Info("Paused: {Position}", mediaElement.Position);
                    await App.PositionData.UpdatePosition(Pos);
                }
                break;
        }
    }

#nullable disable

    #endregion

    protected override void OnDisappearing()
    {
        mediaElement.ShouldKeepScreenOn = false;
        mediaElement.Stop();
    }
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        _logger.Info("Navigating away form Video Player.");
        mediaElement.Stop();
        mediaElement?.Handler.DisconnectHandler();
        base.OnNavigatedFrom(args);
    }

    private void ContentPage_Unloaded(object sender, EventArgs e)
    {
        mediaElement.Stop();
    }
}
