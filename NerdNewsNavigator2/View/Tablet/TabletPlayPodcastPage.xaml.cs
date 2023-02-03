// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View.Tablet;
public partial class TabletPlayPodcastPage : ContentPage
{
    private static System.Timers.Timer s_aTimer;
    readonly PlaybackService _playbackService;
    public TabletPlayPodcastPage(TabletPlayPodcastViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        PlaybackService playbackservice = new(mediaElement);
        _playbackService = playbackservice;

        Start();
    }
    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync($"{nameof(TabletPodcastPage)}");
        return true;
    }
    void ContentPage_Unloaded(object? sender, EventArgs e)
    {
        // Stop and cleanup MediaElement when we navigate away
        mediaElement.Handler?.DisconnectHandler();
    }
    public Task SetTimer()
    {
        s_aTimer = new System.Timers.Timer(2000);
        s_aTimer.Elapsed += _playbackService.OnTimedEvent;
        s_aTimer.Enabled = true;
        return Task.CompletedTask;
    }
    private void Start()
    {
        mediaElement.Pause();
        _playbackService.SetTimer();
        OnPropertyChanged(nameof(mediaElement));
        mediaElement.StateChanged += _playbackService.Media_Stopped;
        mediaElement.PositionChanged += _playbackService.OnPositionChanged;
    }
}
