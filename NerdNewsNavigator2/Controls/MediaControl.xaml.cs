﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Maui.Controls.PlatformConfiguration;

namespace NerdNewsNavigator2.Controls;

public partial class MediaControl : ContentView, IRecipient<ShowItemMessage>
{
    #region Properties
    public bool ShowControls { get; set; }
    public string PlayPosition { get; set; }
    Position Pos { get; set; } = new();

    #endregion
    #region Bindably Properties

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Name), typeof(MediaElement), typeof(MediaControl));
    public static readonly BindableProperty CurrentStateProperty = BindableProperty.Create(nameof(CurrentState), typeof(MediaElementState), typeof(MediaElement), MediaElementState.None);
    public static readonly BindableProperty AspectProperty = BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(MediaElement), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.Aspect = (Aspect)newValue;
    });
    public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(MediaSource), typeof(MediaElement), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.Source = newValue as MediaSource;
    });
    public static readonly BindableProperty StateChangedProperty = BindableProperty.Create(nameof(StateChanged), typeof(EventHandler<MediaStateChangedEventArgs>), typeof(MediaElement), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.StateChanged += (EventHandler<MediaStateChangedEventArgs>)newValue;
    });
    public static readonly BindableProperty MediaOpenedProperty = BindableProperty.Create(nameof(MediaOpened), typeof(EventHandler), typeof(MediaElement), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.MediaOpened += (EventHandler)newValue;
    });
    public static readonly BindableProperty ShouldKeepScreenOnProperty = BindableProperty.Create(nameof(ShouldKeepScreenOn), typeof(bool), typeof(MediaElement), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.ShouldKeepScreenOn = (bool)newValue;
    });
    public static readonly BindableProperty ShouldAutoPlayProperty = BindableProperty.Create(nameof(ShouldAutoPlay), typeof(bool), typeof(MediaElement), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.ShouldAutoPlay = (bool)newValue;
    });
    public static readonly BindableProperty ShouldShowPlaybackControlsProperty = BindableProperty.Create(nameof(ShouldShowPlaybackControls), typeof(bool), typeof(MediaElement), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.ShouldShowPlaybackControls = (bool)newValue;
    });
    public static readonly BindableProperty PositionChangedProperty = BindableProperty.Create(nameof(PositionChanged), typeof(EventHandler<MediaPositionChangedEventArgs>), typeof(MediaElement), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.PositionChanged += (EventHandler<MediaPositionChangedEventArgs>)newValue;
    });
    public static readonly BindableProperty ShouldMuteProperty = BindableProperty.Create(nameof(ShouldMute), typeof(bool), typeof(MediaElement), false, propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.ShouldMute = (bool)newValue;
    });
    public MediaElementState CurrentState
    {
        get => (MediaElementState)GetValue(CurrentStateProperty);
        set => SetValue(CurrentStateProperty, value);
    }
    public bool ShouldMute
    {
        get => (bool)GetValue(ShouldMuteProperty);
        set => SetValue(ShouldMuteProperty, value);
    }
    public EventHandler MediaOpened
    {
        get => GetValue(MediaOpenedProperty) as EventHandler;
        set => SetValue(MediaOpenedProperty, value);
    }
    public EventHandler<MediaStateChangedEventArgs> StateChanged
    {
        get => GetValue(StateChangedProperty) as EventHandler<MediaStateChangedEventArgs>;
        set => SetValue(StateChangedProperty, value);
    }
    public EventHandler<MediaPositionChangedEventArgs> PositionChanged
    {
        get => GetValue(PositionChangedProperty) as EventHandler<MediaPositionChangedEventArgs>;
        set => SetValue(PositionChangedProperty, value);
    }
    public TimeSpan Position
    {
        get => mediaElement.Position;
    }
    public MediaSource Source
    {
        get => GetValue(SourceProperty) as MediaSource;
        set => SetValue(SourceProperty, value);
    }
    public MediaElement Name
    {
        get => GetValue(TitleProperty) as MediaElement;
        set => SetValue(TitleProperty, value);
    }
    public bool ShouldShowPlaybackControls
    {
        get => (bool)GetValue(ShouldShowPlaybackControlsProperty);
        set => SetValue(ShouldShowPlaybackControlsProperty, value);
    }
    public bool ShouldAutoPlay
    {
        get => (bool)GetValue(ShouldAutoPlayProperty);
        set => SetValue(ShouldAutoPlayProperty, value);
    }
    public bool ShouldKeepScreenOn
    {
        get => (bool)GetValue(ShouldKeepScreenOnProperty);
        set => SetValue(ShouldKeepScreenOnProperty, value);
    }
    public Aspect Aspect
    {
        get => (Aspect)GetValue(AspectProperty);
        set => SetValue(AspectProperty, value);
    }
    #endregion
    public MediaControl()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register(this);
        PlayPosition = string.Empty;

        mediaElement.PositionChanged += ChangedPosition;
        mediaElement.PropertyChanged += MediaElement_PropertyChanged;
        mediaElement.PositionChanged += OnPositionChanged;
        _ = Moved();
        BtnPLay.Source = "pause.png";
    }
    #region Methods
    public void SeekTo(TimeSpan position)
    {
        mediaElement.SeekTo(position);
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
    #endregion
    private static string TimeConverter(TimeSpan time)
    {
        var interval = new TimeSpan(time.Hours, time.Minutes, time.Seconds);
        return interval.ToString();
    }
    #region Events
    private void ChangedPosition(object sender, EventArgs e)
    {
        var playDuration = TimeConverter(mediaElement.Duration);
        var position = TimeConverter(mediaElement.Position);
        PlayPosition = $"{position}/{playDuration}";
        OnPropertyChanged(nameof(PlayPosition));
    }

    private void MediaElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == MediaElement.DurationProperty.PropertyName)
        {
            PositionSlider.Maximum = mediaElement.Duration.TotalSeconds;
        }
    }
    private void OnPositionChanged(object sender, MediaPositionChangedEventArgs e)
    {
        PositionSlider.Value = e.Position.TotalSeconds;
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

    public async void Receive(ShowItemMessage message)
    {
        Pos.Title = message.Value.Title;
        Pos.Url = message.Value.Url;
        Pos.SavedPosition = TimeSpan.Zero;
        mediaElement.Source = message.Value.Url;
        mediaElement.Play();
        await Seek(message.Value);
        WeakReferenceMessenger.Default.UnregisterAll(this);
#if WINDOWS
        WeakReferenceMessenger.Default.Register(this);
#endif
    }

    /// <summary>
    /// Manages IOS seeking for <see cref="mediaElement"/> with <see cref="Pos"/> at start of playback.
    /// </summary>
    /// <param name="show"></param>
    private async Task Seek(Show show)
    {
        mediaElement.ShouldKeepScreenOn = true;

        var positionList = await App.PositionData.GetAllPositions();
        var result = positionList.ToList().Find(x => x.Title == show.Title);
        if (result is not null && result.SavedPosition > TimeSpan.FromSeconds(10))
        {
            Pos.SavedPosition = result.SavedPosition;
#if IOS || MACCATALYST
            mediaElement.StateChanged += IOSStart;
#else
            _ = mediaElement.SeekTo(result.SavedPosition);
#endif
        }
        else
        {
            Pos.SavedPosition = mediaElement.Position;
            Pos.Title = show.Title;
            await App.PositionData.UpdatePosition(Pos);
        }
    }
    private async void MediaStopped(object sender, MediaStateChangedEventArgs e)
    {
        switch (e.NewState)
        {
            case MediaElementState.Paused:
                if (mediaElement.Position > TimeSpan.FromSeconds(10))
                {
                    Pos.SavedPosition = mediaElement.Position;
                    await App.PositionData.UpdatePosition(Pos);
                }
                mediaElement.StateChanged += MediaStopped;
                break;
        }
    }
    public void IOSStart(object sender, MediaStateChangedEventArgs e)
    {
        switch (e.NewState)
        {
            case MediaElementState.Playing:
                mediaElement.StateChanged -= IOSStart;
                mediaElement.Pause();
                mediaElement.SeekTo(Pos.SavedPosition);
                mediaElement.Play();
                mediaElement.StateChanged += MediaStopped;
                break;
        }
    }

    #endregion

    private async void MediaControl_Unloaded(object sender, EventArgs e)
    {
        if (mediaElement.Position > TimeSpan.FromSeconds(10) && Pos.Title != string.Empty)
        {
            Pos.SavedPosition = mediaElement.Position;
            await App.PositionData.UpdatePosition(Pos);
        }
        mediaElement.Stop();
#if ANDROID || IOS || MACCATALYST
        mediaElement.StateChanged -= MediaStopped;
        mediaElement.PositionChanged -= ChangedPosition;
        mediaElement.PropertyChanged -= MediaElement_PropertyChanged;
        mediaElement.PositionChanged -= OnPositionChanged;
#endif
    }
}
