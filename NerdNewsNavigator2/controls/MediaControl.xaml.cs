// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.controls;

public partial class MediaControl : ContentView
{
    public string PlayPosition { get; set; }
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Name), typeof(MediaElement), typeof(MediaControl), propertyChanged: (bindable, oldValue, newValue) =>
        {
            var control = (MediaControl)bindable;
            control.Source = newValue as string;
            control.ShouldAutoPlay = (bool)newValue;
            control.ShouldKeepScreenOn = (bool)newValue;
            control.ShouldShowPlaybackControls = (bool)newValue;
        });
    public static readonly BindableProperty ButtonProperty = BindableProperty.Create(nameof(ButtonName), typeof(ImageButton), typeof(MediaControl), propertyChanged: (bindable, oldValue, newValue) =>
    {
        var control = (MediaControl)bindable;
        control.Command = newValue as string;
    });
    public MediaControl()
    {
        InitializeComponent();
        PlayPosition = string.Empty;
        mediaElement.PropertyChanged += MediaElement_PropertyChanged;
        mediaElement.PositionChanged += ChangedPosition;
    }
    public ImageButton ButtonName
    {
        get => (ImageButton)GetValue(ButtonProperty);
        set => SetValue(ButtonProperty, value);
    }
    public string Command
    {
        get => (string)GetValue(ButtonProperty);
        set => SetValue(ButtonProperty, value);
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
    public string Source
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

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
        /*
        if (e.Direction == SwipeDirection.Up)
        {
            App.SetFullScreen(sender, e);
        }
        if (e.Direction == SwipeDirection.Down)
        {
            App.RestoreScreen(sender, e);
        }
        */
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
}
