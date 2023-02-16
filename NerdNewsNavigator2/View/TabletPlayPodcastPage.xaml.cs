// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;
public partial class TabletPlayPodcastPage : ContentPage
{
    readonly PlaybackService _playbackService;
    public TabletPlayPodcastPage(TabletPlayPodcastViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        PlaybackService playbackService = new(mediaElement);
        _playbackService = playbackService;
    }

#nullable enable
    void ContentPage_Unloaded(object? sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        System.Diagnostics.Debug.WriteLine("Unloading media element");

        mediaElement.MediaOpened -= _playbackService.Slider_DragCompleted;
        mediaElement.StateChanged -= _playbackService.Media_Stopped;
        // Stop and cleanup MediaElement when we navigate away

#if ANDROID
mediaElement.Handler?.DisconnectHandler();
#endif

    }

#nullable disable
}
