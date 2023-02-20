// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;
public partial class TabletPlayPodcastPage : ContentPage
{
    private Position Pos { get; set; } = new();
    private bool isPlaying = false;
    public TabletPlayPodcastPage(TabletPlayPodcastViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
#if WINDOWS || ANDROID
mediaElement.MediaOpened += Seek;
#endif
#if IOS
        mediaElement.StateChanged += SeekIOS;
#endif
    }

#nullable enable
    public async void Media_Stopped(object? sender, MediaStateChangedEventArgs e)
    {
        if (sender is null || isPlaying == false)
        {
            return;
        }
        if ((mediaElement.CurrentState == MediaElementState.Paused) && mediaElement.Position != Pos.SavedPosition)
        {
            Pos.SavedPosition = mediaElement.Position;
            await Save();
        }
    }
    public async void Seek(object? sender, EventArgs e)
    {
        if (sender is null && isPlaying == false)
        {
            return;
        }
        Pos.Title = Preferences.Default.Get("New_Url", string.Empty);
        Pos.SavedPosition = TimeSpan.Zero;
        var positionList = await App.PositionData.GetAllPositions();
        foreach (var item in positionList)
        {
            if (Pos.Title == item.Title)
            {
                Pos.SavedPosition = item.SavedPosition;
            }
        }
        mediaElement.SeekTo(Pos.SavedPosition);
        isPlaying = true;
        mediaElement.StateChanged += Media_Stopped;
    }
    public async void SeekIOS(object sender, MediaStateChangedEventArgs e)
    {
        Pos.Title = Preferences.Default.Get("New_Url", string.Empty);
        Pos.SavedPosition = TimeSpan.Zero;
        var positionList = await App.PositionData.GetAllPositions();
        foreach (var item in positionList)
        {
            if (Pos.Title == item.Title)
            {
                Pos.SavedPosition = item.SavedPosition;
            }
        }
        if (e.NewState == MediaElementState.Playing)
        {
            mediaElement.SeekTo(Pos.SavedPosition);
        }
        isPlaying = true;
        mediaElement.StateChanged += Media_Stopped;
    }
    public async void SeekIOS(object sender, MediaStateChangedEventArgs e)
    {
        Pos.Title = Preferences.Default.Get("New_Url", string.Empty);
        Pos.SavedPosition = TimeSpan.Zero;
        var positionList = await App.PositionData.GetAllPositions();
        foreach (var item in positionList)
        {
            //Debug.WriteLine($"searching in: {item.Title} at: {item.SavedPosition.TotalSeconds}");
            if (Pos.Title == item.Title)
            {
                Pos.SavedPosition = item.SavedPosition;
                //Debug.WriteLine($"Found: {item.Title} at: {item.SavedPosition.TotalSeconds}");
            }
        }
        if (e.NewState == MediaElementState.Playing)
        {
            mediaElement.SeekTo(Pos.SavedPosition);
        }
        isPlaying = true;
        mediaElement.StateChanged += Media_Stopped;
        //Debug.WriteLine($"Seeking {Pos.Title} at: {Pos.SavedPosition.TotalSeconds}");
    }

    private async Task Save()
    {
        await App.PositionData.Add(new Position
        {
            Title = Pos.Title,
            SavedPosition = Pos.SavedPosition,
        });
    }
    private void ContentPage_Unloaded(object? sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        // Stop and cleanup MediaElement when we navigate away
        mediaElement.MediaOpened -= Seek;
        mediaElement.StateChanged -= Media_Stopped;
        mediaElement.Handler?.DisconnectHandler();
    }
}
