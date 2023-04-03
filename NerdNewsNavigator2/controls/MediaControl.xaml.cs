// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Controls;

public partial class MediaControl : ContentView
{
    #region Properties and Bindable Properties
    public string PlayPosition { get; set; }
    public bool MenuIsVisible { get; set; } = false;

    private static bool s_fullScreen = false;

    public bool FullScreen { get; set; } = false;

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Name), typeof(MediaElement), typeof(MediaControl), propertyChanged: (bindable, oldValue, newValue) =>
        {
            var control = (MediaControl)bindable;
            control.mediaElement.ShouldAutoPlay = (bool)newValue;
            control.mediaElement.ShouldKeepScreenOn = (bool)newValue;
            control.mediaElement.Source = newValue as MediaSource;
            control.mediaElement.ShouldShowPlaybackControls = (bool)newValue;
            control.mediaElement.PositionChanged += (EventHandler<MediaPositionChangedEventArgs>)newValue;
            control.mediaElement.StateChanged += (EventHandler<MediaStateChangedEventArgs>)newValue;
            control.mediaElement.MediaOpened += (EventHandler)newValue;
        });

    public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(MediaSource), typeof(MediaControl), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.Source = newValue as MediaSource;
    });
    public static readonly BindableProperty StateChangedProperty = BindableProperty.Create(nameof(StateChanged), typeof(EventHandler<MediaStateChangedEventArgs>), typeof(MediaControl), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.StateChanged += (EventHandler<MediaStateChangedEventArgs>)newValue;
    });
    public static readonly BindableProperty MediaOpenedProperty = BindableProperty.Create(nameof(MediaOpened), typeof(EventHandler), typeof(MediaControl), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.MediaOpened += (EventHandler)newValue;
    });
    public static readonly BindableProperty ShouldKeepScreenOnProperty = BindableProperty.Create(nameof(ShouldKeepScreenOn), typeof(bool), typeof(MediaControl), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.ShouldKeepScreenOn = (bool)newValue;
    });
    public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(TimeSpan), typeof(MediaElement), TimeSpan.Zero);
    public static readonly BindableProperty ShouldAutoPlayProperty = BindableProperty.Create(nameof(ShouldAutoPlay), typeof(bool), typeof(MediaControl), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.ShouldAutoPlay = (bool)newValue;
    });
    public static readonly BindableProperty ShouldShowPlaybackControlsProperty = BindableProperty.Create(nameof(ShouldShowPlaybackControls), typeof(bool), typeof(MediaControl), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.ShouldShowPlaybackControls = (bool)newValue;
    });
    public static readonly BindableProperty PositionChangedProperty = BindableProperty.Create(nameof(PositionChanged), typeof(EventHandler<MediaPositionChangedEventArgs>), typeof(MediaControl), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaControl)bindableProperty;
        control.mediaElement.PositionChanged += (EventHandler<MediaPositionChangedEventArgs>)newValue;
    });
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
    public MediaSource Source
    {
        get => GetValue(SourceProperty) as MediaSource;
        set => SetValue(SourceProperty, value);
    }
    public TimeSpan Position => mediaElement.Position;
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

    #endregion
    public MediaControl()
    {
        InitializeComponent();
        PlayPosition = string.Empty;
        mediaElement.PropertyChanged += MediaElement_PropertyChanged;
        mediaElement.PositionChanged += ChangedPosition;
        mediaElement.PositionChanged += OnPositionChanged;
        _ = Moved();
        OnPropertyChanged(nameof(MenuIsVisible));
    }
    public void SeekTo(TimeSpan position)
    {
        mediaElement.Pause();
        mediaElement.SeekTo(position);
        mediaElement.Play();
    }
    public void Play()
    {
        mediaElement.Play();
    }
    public void Pause()
    {
        mediaElement.Pause();
    }
    public void Stop()
    {
        mediaElement.Stop();
    }

    #region Events

#nullable enable
    private void MediaElement_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == MediaElement.DurationProperty.PropertyName)
        {
            PositionSlider.Maximum = mediaElement.Duration.TotalSeconds;
        }
    }
    private void OnPositionChanged(object? sender, MediaPositionChangedEventArgs e)
    {
        PositionSlider.Value = e.Position.TotalSeconds;
    }
    private void Slider_DragCompleted(object? sender, EventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);

        var newValue = ((Slider)sender).Value;
        mediaElement.SeekTo(TimeSpan.FromSeconds(newValue));
        mediaElement.Play();
    }

#nullable disable
    private void Slider_DragStarted(object sender, EventArgs e)
    {
        mediaElement.Pause();
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
        if (mediaElement.CurrentState == MediaElementState.Stopped ||
       mediaElement.CurrentState == MediaElementState.Paused)
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

    private static void BtnFullScreen_Clicked(object sender, EventArgs e)
    {
        if (s_fullScreen)
        {
            s_fullScreen = false;
            RestoreScreen();
        }
        else
        {
            SetFullScreen();
            s_fullScreen = true;
        }
    }
    private void OnMuteClicked(object sender, EventArgs e)
    {
        mediaElement.ShouldMute = !mediaElement.ShouldMute;
        if (mediaElement.ShouldMute)
        {
            ImageButtonMute.Source = "mute.png";
        }
        else
        {
            ImageButtonMute.Source = "muted.png";
        }
        OnPropertyChanged(nameof(ImageButtonMute.Source));
    }
    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        _ = Moved();
    }
    private static void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
    {
        if (e.Direction == SwipeDirection.Up)
        {
            SetFullScreen();
        }
        if (e.Direction == SwipeDirection.Down)
        {
            RestoreScreen();
        }
    }
    private void Button_Pressed(object sender, EventArgs e)
    {
        if (MenuIsVisible)
        {
            MenuIsVisible = false;
        }
        else
        {
            MenuIsVisible = true;
        }
        OnPropertyChanged(nameof(MenuIsVisible));
    }
    #endregion

    #region Full Screen Functions
#nullable enable

    /// <summary>
    /// Method toggles Full Screen Off
    /// </summary>
    public static void RestoreScreen()
    {
        DeviceService.RestoreScreen();
    }

    /// <summary>
    /// Method toggles Full Screen On
    /// </summary>

    public static void SetFullScreen()
    {
        DeviceService.FullScreen();
    }

    private async Task Moved()
    {
        if (!FullScreen)
        {
            FullScreen = true;
            OnPropertyChanged(nameof(FullScreen));
            await Task.Delay(7000);
            FullScreen = false;
            MenuIsVisible = false;
            OnPropertyChanged(nameof(MenuIsVisible));
            OnPropertyChanged(nameof(FullScreen));
        }
    }
    #endregion

    /// <summary>
    /// Manages unload event from <see cref="mediaElement"/> after it is unloaded.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContentView_Unloaded(object sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        mediaElement.ShouldKeepScreenOn = false;
        mediaElement.Stop();
        // Stop and cleanup MediaElement when we navigate away
        mediaElement.Handler?.DisconnectHandler();
    }

    /// <summary>
    /// A method that converts <see cref="TimeSpan"/> into a usable <see cref="string"/> for displaying position in <see cref="MediaElement"/>
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private static string TimeConverter(TimeSpan time)
    {
        var interval = new TimeSpan(time.Hours, time.Minutes, time.Seconds);
        return (interval).ToString();
    }

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="LivePage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public void Tapped(string url)
    {
        mediaElement.Stop();
        mediaElement.Source = url;
        MenuIsVisible = false;
        mediaElement.Play();
        OnPropertyChanged(nameof(MenuIsVisible));
    }

    private async void PointerGestureRecognizer_PointerMoved(object sender, PointerEventArgs e)
    {
        await Task.Delay(5000);
        MenuIsVisible = false;
        OnPropertyChanged(nameof(MenuIsVisible));
    }
}
