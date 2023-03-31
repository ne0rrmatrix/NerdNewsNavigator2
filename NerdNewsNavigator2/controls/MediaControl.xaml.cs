// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Core;
#endif

namespace NerdNewsNavigator2.Controls;

public partial class MediaControl : ContentView
{
    #region Properties and Bindable Properties
    public string PlayPosition { get; set; }
    public Page CurrentPage { get; set; }
    public static TimeSpan CurrentPositon { get; set; }

    private static bool s_fullScreen = false;

#if WINDOWS
    private static MauiWinUIWindow CurrentWindow { get; set; }
#endif

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
    public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(TimeSpan), typeof(MediaControl), TimeSpan.Zero);
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
    public TimeSpan Position => (TimeSpan)GetValue(PositionProperty);
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
        CurrentPage = Shell.Current.CurrentPage;
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
    private static void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
    {
#if WINDOWS
        CurrentWindow = BaseViewModel.CurrentWindow;
#endif
        if (e.Direction == SwipeDirection.Up)
        {
            SetFullScreen();
        }
        if (e.Direction == SwipeDirection.Down)
        {
            RestoreScreen();
        }
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
        var playDuration = BaseViewModel.TimeConverter(mediaElement.Duration);
        var position = BaseViewModel.TimeConverter(mediaElement.Position);
        PlayPosition = $"{position}/{playDuration}";
        CurrentPositon = mediaElement.Position;
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
#if WINDOWS
        CurrentWindow = BaseViewModel.CurrentWindow;
#endif
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
    #endregion

    #region Full Screen Functions
#nullable enable
    /// <summary>
    /// Method toggles Full Screen Off
    /// </summary>
    public static void RestoreScreen()
    {
#if WINDOWS
        if (CurrentWindow is not null)
        {
            var appWindow = GetAppWindow(CurrentWindow);

            switch (appWindow.Presenter)
            {
                case Microsoft.UI.Windowing.OverlappedPresenter overlappedPresenter:
                    if (overlappedPresenter.State == Microsoft.UI.Windowing.OverlappedPresenterState.Maximized)
                    {
                        overlappedPresenter.SetBorderAndTitleBar(true, true);
                        overlappedPresenter.Restore();
                    }
                    break;
            }
        }
#endif

#if ANDROID
        var activity = Platform.CurrentActivity;

        if (activity == null || activity.Window == null) return;

        Views.WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);
        var windowInsetsControllerCompat = Views.WindowCompat.GetInsetsController(activity.Window, activity.Window.DecorView);
        var types = Views.WindowInsetsCompat.Type.StatusBars() |
                    Views.WindowInsetsCompat.Type.NavigationBars();
        windowInsetsControllerCompat.Show(types);
#endif
    }

    /// <summary>
    /// Method toggles Full Screen On
    /// </summary>

    public static void SetFullScreen()
    {

#if ANDROID
        var activity = Platform.CurrentActivity;

        if (activity == null || activity.Window == null) return;

        Views.WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);
        var windowInsetsControllerCompat = Views.WindowCompat.GetInsetsController(activity.Window, activity.Window.DecorView);
        var types = Views.WindowInsetsCompat.Type.StatusBars() |
                    Views.WindowInsetsCompat.Type.NavigationBars();

        windowInsetsControllerCompat.SystemBarsBehavior = Views.WindowInsetsControllerCompat.BehaviorShowBarsBySwipe;
        windowInsetsControllerCompat.Hide(types);
#endif

#if WINDOWS
        if (CurrentWindow is not null)
        {
            var appWindow = GetAppWindow(CurrentWindow);
            switch (appWindow.Presenter)
            {
                case Microsoft.UI.Windowing.OverlappedPresenter overlappedPresenter:
                    overlappedPresenter.SetBorderAndTitleBar(false, false);
                    overlappedPresenter.Maximize();
                    break;
            }
        }
#endif
    }

#if WINDOWS
    /// <summary>
    /// Method is required for switching Full Screen Mode for Windows
    /// </summary>
    private static Microsoft.UI.Windowing.AppWindow GetAppWindow(MauiWinUIWindow window)
    {
        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
        return appWindow;
    }

#endif
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
        // Stop and cleanup MediaElement when we navigate away
        mediaElement.Handler?.DisconnectHandler();
    }
}
