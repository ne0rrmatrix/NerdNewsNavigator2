﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Maui.Controls.PlatformConfiguration;
using NerdNewsNavigator2.controls;
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
using System.Data;
#endif

namespace NerdNewsNavigator2.controls;

public partial class MediaControl : ContentView
{
    public Page CurrentPage { get; set; }

    private bool _fullScreen = false;
#if WINDOWS
    private static MauiWinUIWindow CurrentWindow { get; set; }
#endif
    public string PlayPosition { get; set; }
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Name), typeof(MediaElement), typeof(MediaElement), propertyChanged: (bindable, oldValue, newValue) =>
        {
            var control = (MediaControl)bindable;
            control.ShouldAutoPlay = (bool)newValue;
            control.ShouldKeepScreenOn = (bool)newValue;
            control.ShouldShowPlaybackControls = (bool)newValue;
        });
    public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Name), typeof(MediaElement), typeof(MediaElement), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaElement)bindableProperty;
        control.Source = (MediaSource)newValue;
    });
    public static readonly BindableProperty NameProperty = BindableProperty.Create(nameof(Name), typeof(MediaElement), typeof(MediaElement), propertyChanged: (bindableProperty, oldValue, newValue) =>
    {
        var control = (MediaElement)bindableProperty;
    });
    public MediaElement Name
    {
        get => (MediaElement)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }
    public MediaSource Source
    {
        get => (MediaSource)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    public MediaControl()
    {
        InitializeComponent();
        PlayPosition = string.Empty;
        mediaElement.PropertyChanged += MediaElement_PropertyChanged;
        mediaElement.PositionChanged += ChangedPosition;
        CurrentPage = Shell.Current.CurrentPage;

    }
    public TimeSpan Position
    {
        get => (TimeSpan)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);

    }
    public MediaElement Name
    {
        get => (MediaElement)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public bool ShouldShowPlaybackControls
    {
        get => (bool)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public bool ShouldAutoPlay
    {
        get => (bool)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public bool ShouldKeepScreenOn
    {
        get => (bool)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public Action<object, EventArgs> MediaOpened { get; set; }
    public Action<object, MediaStateChangedEventArgs> StateChanged { get; set; }

    /// <summary>
    /// Method returns 720P URL for <see cref="mediaElement"/> to Play.
    /// </summary>
    /// <param name="m3UString"></param>
    /// <returns></returns>
    public static string ParseM3UPLaylist(string m3UString)
    {
        var masterPlaylist = MasterPlaylist.LoadFromText(m3UString);
        var list = masterPlaylist.Streams.ToList();
        return list.ElementAt(list.FindIndex(x => x.Resolution.Height == 720)).Uri;
    }

    /// <summary>
    /// Method returns the Live stream M3U Url from youtube ID.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static async Task<string> GetM3U_Url(string url)
    {
        var content = string.Empty;
        var client = new HttpClient();
        var youtube = new YoutubeClient();
        var result = await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(url);
        var response = await client.GetAsync(result);
        if (response.IsSuccessStatusCode)
        {
            content = await response.Content.ReadAsStringAsync();
        }

        return content;
    }
    private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
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
    public void SeekTo(TimeSpan position)
    {
        mediaElement.SeekTo(position);
    }
    public void Stop()
    {
        mediaElement.Stop();
    }
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
#nullable disable
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
    private void ChangedPosition(object sender, EventArgs e)
    {
        var playDuration = BaseViewModel.TimeConverter(mediaElement.Duration);
        var position = BaseViewModel.TimeConverter(mediaElement.Position);
        PlayPosition = $"{position}/{playDuration}";
        OnPropertyChanged(nameof(PlayPosition));
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
#nullable enable
    void OnMuteClicked(object? sender, EventArgs e)
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
    void Slider_DragCompleted(object? sender, EventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);

        var newValue = ((Slider)sender).Value;
        mediaElement.SeekTo(TimeSpan.FromSeconds(newValue));
        mediaElement.Play();
    }
#nullable disable
    void Slider_DragStarted(object sender, EventArgs e)
    {
        mediaElement.Pause();
    }
    public void LoadUrl(string url)
    {
        mediaElement.Source = url;
    }
    /// <summary>
    /// Method Starts <see cref="MediaElement"/> Playback.
    /// </summary>
    /// <returns></returns>
    public async Task LoadVideo()
    {
        var m3u = await GetM3U_Url("F2NreNEmMy4");
        mediaElement.Source = ParseM3UPLaylist(m3u);
        mediaElement.Play();
    }

#nullable enable
    /// <summary>
    /// Method toggles Full Screen Off
    /// </summary>
    public void RestoreScreen()
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

    public void SetFullScreen()
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

    private void btnFullScreen_Clicked(object sender, EventArgs e)
    {
#if WINDOWS
        CurrentWindow = BaseViewModel.CurrentWindow;
#endif
        if (_fullScreen)
        {
            _fullScreen = false;
            RestoreScreen();
        }
        else
        {
            SetFullScreen();
            _fullScreen = true;
        }
    }

#if WINDOWS
    /// <summary>
    /// Method is required for switching Full Screen Mode for Windows
    /// </summary>
    private Microsoft.UI.Windowing.AppWindow GetAppWindow(MauiWinUIWindow window)
    {
        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
        return appWindow;
    }
#endif
#nullable disable
}