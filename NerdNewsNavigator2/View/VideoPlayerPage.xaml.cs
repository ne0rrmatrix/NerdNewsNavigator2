﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core.Platform;

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that Displays a Video from twit.tv.
/// </summary>
public partial class VideoPlayerPage : ContentPage, IRecipient<ShowItemMessage>
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
        WeakReferenceMessenger.Default.Register(this);
        PlayPosition = string.Empty;
        BtnPLay.Source = "pause.png";
    }

    #region Events
    public async void Receive(ShowItemMessage message)
    {
        PlayPosition = string.Empty;
        BtnPLay.Source = "pause.png";
        _ = Moved();
        mediaElement.PositionChanged += ChangedPosition;
        mediaElement.PropertyChanged += MediaElement_PropertyChanged;
        mediaElement.PositionChanged += OnPositionChanged;
        _logger.Info($"Navigated: {message.Value.Url}");
        Pos.Title = message.Value.Title;
        Pos.Url = message.Value.Url;
        Pos.SavedPosition = TimeSpan.Zero;
        mediaElement.Source = message.Value.Url;
        mediaElement.Play();
        await Seek(message.Value);
        WeakReferenceMessenger.Default.UnregisterAll(this);
        WeakReferenceMessenger.Default.Register(this);
    }

#if ANDROID || IOS16_1_OR_GREATER
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
#pragma warning disable CA1416 // Validate platform compatibility
        this.Behaviors.Add(new StatusBarBehavior
        {
            StatusBarColor = Color.FromArgb("#000000")
        });
#pragma warning restore CA1416 // Validate platform compatibility
    }
#endif
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        mediaElement.PositionChanged -= ChangedPosition;
        mediaElement.PropertyChanged -= MediaElement_PropertyChanged;
        mediaElement.PositionChanged -= OnPositionChanged;
        mediaElement.StateChanged -= MediaStopped;
        _logger.Info("Media has finished playing.");
        mediaElement.ShouldKeepScreenOn = false;
        _logger.Info("ShouldKeepScreenOn set to false.");
        if (mediaElement.Position > TimeSpan.FromSeconds(10))
        {
            Pos.SavedPosition = mediaElement.Position;
            _ = App.PositionData.UpdatePosition(Pos);
        }
#if ANDROID || IOS16_1_OR_GREATER
#pragma warning disable CA1416 // Validate platform compatibility
        this.Behaviors.Add(new StatusBarBehavior
        {
            StatusBarColor = Color.FromArgb("#34AAD2")
        });
#pragma warning restore CA1416 // Validate platform compatibility
#endif
        _logger.Info("Navigating away form Video Player.");
        mediaElement.Stop();
        base.OnNavigatedFrom(args);
    }

    private void MediaElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
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

    private void OnPositionChanged(object sender, MediaPositionChangedEventArgs e)
    {
        PositionSlider.Value = e.Position.TotalSeconds;
    }

    /// <summary>
    /// Manages the saving of <see cref="Position"/> data in <see cref="PositionDataBase"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MediaStopped(object sender, MediaStateChangedEventArgs e)
    {
        switch (e.NewState)
        {
            case MediaElementState.Paused:
                _logger.Info($"Paused: {mediaElement.Position}");
                if (mediaElement.Position > TimeSpan.FromSeconds(10))
                {
                    Pos.SavedPosition = mediaElement.Position;
                    await App.PositionData.UpdatePosition(Pos);
                }
                break;
        }
    }

    private void ChangedPosition(object sender, EventArgs e)
    {
        var playDuration = TimeConverter(mediaElement.Duration);
        var position = TimeConverter(mediaElement.Position);
        PlayPosition = $"{position}/{playDuration}";
        OnPropertyChanged(nameof(PlayPosition));
    }
    #endregion

    #region Buttons
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

    private void TapGestureRecognizer_DoubleTapped(object sender, TappedEventArgs e)
    {
#if WINDOWS
        SetFullScreenStatus();
#endif
    }

    #endregion

    private void SetFullScreenStatus()
    {
        grid.Margin = grid.Margin.IsEmpty ? new Thickness(10) : new Thickness(0);
        CustomControls.SetFullScreenStatus();
    }

    private async Task Moved()
    {
        ShowControls = true;
        OnPropertyChanged(nameof(ShowControls));
        await Task.Delay(7000);
        ShowControls = false;
        OnPropertyChanged(nameof(ShowControls));
    }

    private static string TimeConverter(TimeSpan time)
    {
        var interval = new TimeSpan(time.Hours, time.Minutes, time.Seconds);
        return interval.ToString();
    }

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
        if (result is not null && result.SavedPosition > TimeSpan.FromSeconds(10))
        {
            Pos.SavedPosition = result.SavedPosition;
            _logger.Info($"Retrieved Saved position from database is: {Pos.Title} - {Pos.SavedPosition}");
#if IOS || MACCATALYST
            mediaElement.StateChanged += IOSStart;
#else
            await mediaElement.SeekTo(Pos.SavedPosition);
            mediaElement.StateChanged += MediaStopped;
#endif
        }
        else
        {
            Pos.SavedPosition = mediaElement.Position;
            await App.PositionData.AddPosition(Pos);
            _logger.Info("Could not find saved position");
            mediaElement.StateChanged += MediaStopped;
        }
    }

    public async void IOSStart(object sender, MediaStateChangedEventArgs e)
    {
        switch (e.NewState)
        {
            case MediaElementState.Playing:
                mediaElement.StateChanged -= IOSStart;
                mediaElement.Pause();
                await mediaElement.SeekTo(Pos.SavedPosition);
                mediaElement.Play();
                mediaElement.StateChanged += MediaStopped;
                break;
        }
    }
}
