// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Maui.Core.Platform;
namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that Displays a Video from twit.tv.
/// </summary>
public partial class VideoPlayerPage : ContentPage
{
    #region Properties
    public bool ShowControls { get; set; }
    public string PlayPosition { get; private set; }
    /// <summary>
    /// Initializes a new instance of the <see cref="ILogger"/> class
    /// </summary>
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(VideoPlayerPage));

    /// <summary>
    /// Initializes a new instance of the <see cref="Position"/> class
    /// </summary>
    private Position Pos { get; set; } = new();

    #endregion
    /// <summary>
    /// Class Constructor that initializes <see cref="VideoPlayerPage"/>
    /// </summary>
    /// <param name="viewModel">This Applications <see cref="VideoPlayerPage"/> instance is managed through this class.</param>

    public VideoPlayerPage(VideoPlayerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        PlayPosition = string.Empty;
        mediaElement.PositionChanged += ChangedPosition;
        mediaElement.PropertyChanged += MediaElement_PropertyChanged;
        mediaElement.PositionChanged += OnPositionChanged;
        _ = Moved();
        BtnPLay.Source = "pause.png";
        App.OnVideoNavigated.Navigation += Now;
    }

#nullable enable
    private async void Now(object? sender, VideoNavigationEventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        _logger.Info($"Navigated: {e.CurrentShow.Url}");
        App.OnVideoNavigated.Navigation -= Now;
        Pos.Title = e.CurrentShow.Title;
        await Seek(e.CurrentShow);
    }

    #region Events
    /// <summary>
    /// Manages IOS seeking for <see cref="mediaElement"/> with <see cref="Pos"/> at start of playback.
    /// </summary>
    /// <param name="show"></param>
    private async Task Seek(Show show)
    {
        _logger.Info($"Title: {show.Title}");
        mediaElement.ShouldKeepScreenOn = true;
        var positionList = await App.PositionData.GetAllPositions();
        var result = positionList.ToList().Find(x => x.Title == show.Title);
        if (result is not null)
        {
            Pos.SavedPosition = result.SavedPosition;
            mediaElement.Pause();
            _logger.Info($"Retrieved Saved position from database is: {Pos.Title} - {Pos.SavedPosition}");
            await mediaElement.SeekTo(Pos.SavedPosition);
            mediaElement.Play();
            mediaElement.StateChanged += MediaStopped;
        }
        else
        {
            Pos.SavedPosition = mediaElement.Position;
            await App.PositionData.AddPosition(Pos);
            _logger.Info("Could not find saved position");
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
                _logger.Info("Media has finished playing.");
                mediaElement.ShouldKeepScreenOn = false;
                _logger.Info("ShouldKeepScreenOn set to false.");
                Pos.SavedPosition = mediaElement.Position;
                await App.PositionData.UpdatePosition(Pos);
                break;
            case MediaElementState.Paused:
                _logger.Info($"Paused: {mediaElement.Position}");
                _logger.Info("Media paused. Setting should keep screen on to false");
                mediaElement.ShouldKeepScreenOn = false;
                Pos.SavedPosition = mediaElement.Position;
                await App.PositionData.UpdatePosition(Pos);
                break;
            case MediaElementState.Playing:
                mediaElement.ShouldKeepScreenOn = true;
                _logger.Info("Setting should keep screen on to true");
                break;
        }
    }

#nullable disable

    #endregion

    protected override void OnDisappearing()
    {
        mediaElement.Stop();
    }
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        _logger.Info("Navigating away form Video Player.");
        mediaElement.Stop();
        mediaElement.Handler.DisconnectHandler();
#pragma warning disable CA1416
#if ANDROID || IOS16_1_OR_GREATER || MACCATALYST14_3_OR_GREATER
        var color = Color.FromArgb("#34AAD2");
        StatusBar.SetColor(color);
#endif
#pragma warning restore CA1416
        base.OnNavigatedFrom(args);
    }

    private void ContentPage_Unloaded(object sender, EventArgs e)
    {
        mediaElement.Stop();
    }
    public void Play()
    {
        mediaElement.Play();
        BtnPLay.Source = "pause.png";
    }
    public void Pause()
    {
        mediaElement.Pause();
        BtnPLay.Source = "play.png";
    }
    public void Stop()
    {
        mediaElement.Stop();
        BtnPLay.Source = "pause.png";
    }
    private async Task Moved()
    {
        ShowControls = true;
        OnPropertyChanged(nameof(ShowControls));
        await Task.Delay(7000);
        ShowControls = false;
        OnPropertyChanged(nameof(ShowControls));
    }
    #region Events
#nullable enable
    private void MediaElement_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        if (e.PropertyName == MediaElement.DurationProperty.PropertyName)
        {
            PositionSlider.Maximum = mediaElement.Duration.TotalSeconds;
        }
    }
    private void Slider_DragCompleted(object sender, EventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);
        var newValue = ((Slider)sender).Value;
        mediaElement.SeekTo(TimeSpan.FromSeconds(newValue));
        mediaElement.Play();
    }
    private void Slider_DragStarted(object sender, EventArgs e)
    {
        mediaElement.Pause();
    }
    private void OnPositionChanged(object? sender, MediaPositionChangedEventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        PositionSlider.Value = e.Position.TotalSeconds;
    }
    private void ChangedPosition(object? sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        var playDuration = TimeConverter(mediaElement.Duration);
        var position = TimeConverter(mediaElement.Position);
        PlayPosition = $"{position}/{playDuration}";
        OnPropertyChanged(nameof(PlayPosition));
    }
    private static string TimeConverter(TimeSpan time)
    {
        var interval = new TimeSpan(time.Hours, time.Minutes, time.Seconds);
        return interval.ToString();
    }

#nullable disable
    #endregion

    #region Buttons
    private void BtnRewind_Clicked(object sender, EventArgs e)
    {
        var time = mediaElement.Position - TimeSpan.FromSeconds(15);
        mediaElement.Pause();
        mediaElement.SeekTo(time);
        mediaElement.Play();
    }

    private void BtnForward_Clicked(object sender, EventArgs e)
    {
        var time = mediaElement.Position + TimeSpan.FromSeconds(15);
        mediaElement.Pause();
        mediaElement.SeekTo(time);
        mediaElement.Play();
    }
    private void BtnPlay_Clicked(object sender, EventArgs e)
    {
        if (mediaElement.CurrentState is MediaElementState.Stopped or
       MediaElementState.Paused)
        {
            mediaElement.Play();
            BtnPLay.Source = "pause.png";
        }
        else if (mediaElement.CurrentState == MediaElementState.Playing)
        {
            mediaElement.Pause();
            BtnPLay.Source = "play.png";
        }
    }
    private void BtnFullScreen_Clicked(object sender, EventArgs e)
    {
        SetFullScreenStatus();
    }
    private void OnMuteClicked(object sender, EventArgs e)
    {
        mediaElement.ShouldMute = !mediaElement.ShouldMute;
        ImageButtonMute.Source = mediaElement.ShouldMute ? (ImageSource)"mute.png" : (ImageSource)"muted.png";
        OnPropertyChanged(nameof(ImageButtonMute.Source));
    }
    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        _ = Moved();
    }
    private void AspectButton(object sender, EventArgs e)
    {
        mediaElement.Aspect = mediaElement.Aspect == Aspect.AspectFit ? Aspect.AspectFill : Aspect.AspectFit;
    }
    #endregion

    #region Full Screen Functions
    private void TapGestureRecognizer_DoubleTapped(object sender, TappedEventArgs e)
    {
#if WINDOWS
        SetFullScreenStatus();
#endif
    }

    private void SetFullScreenStatus()
    {
        if (grid.Margin.IsEmpty)
        {
            grid.Margin = new Thickness(10);
        }
        else
        {
            grid.Margin = new Thickness(0);
        }
        CustomControls.SetFullScreenStatus();
    }

    private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
    {
        if (e.Direction == SwipeDirection.Up)
        {
            CustomControls.FullScreen();
            grid.Margin = new Thickness(0);
        }
        if (e.Direction == SwipeDirection.Down)
        {
            CustomControls.RestoreScreen();
            grid.Margin = new Thickness(10);
        }
    }
    #endregion
}
