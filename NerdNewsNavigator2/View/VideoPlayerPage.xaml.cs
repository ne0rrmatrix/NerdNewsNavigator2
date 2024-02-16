// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if ANDROID || IOS16_1_OR_GREATER
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core.Platform;
#endif

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that Displays a Video from twit.tv.
/// </summary>
public partial class VideoPlayerPage : ContentPage, IRecipient<ShowItemMessage>
{
    Position Pos { get; set; } = new();

    /// <summary>
    /// Class Constructor that initializes <see cref="VideoPlayerPage"/>
    /// </summary>
    /// <param name="viewModel">This Applications <see cref="VideoPlayerPage"/> instance is managed through this class.</param>

    public VideoPlayerPage(VideoPlayerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        WeakReferenceMessenger.Default.Register(this);
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
            await mediaElement.SeekTo(result.SavedPosition);
            mediaElement.StateChanged += MediaStopped;
#endif
        }
        else
        {
            Pos.SavedPosition = mediaElement.Position;
            Pos.Title = show.Title;
            await App.PositionData.UpdatePosition(Pos);
            mediaElement.StateChanged += MediaStopped;
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
#endif
    }

#if ANDROID || IOS16_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
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

#if ANDROID || IOS16_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        this.Behaviors.Add(new StatusBarBehavior
        {
            StatusBarColor = Color.FromArgb("#34AAD2")
        });
#pragma warning restore CA1416 // Validate platform compatibility
        base.OnNavigatedFrom(args);
    }
#endif
}
