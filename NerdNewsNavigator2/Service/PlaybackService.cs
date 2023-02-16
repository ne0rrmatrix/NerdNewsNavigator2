// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;
public class PlaybackService
{
    private readonly MediaElement _mediaElement;
    public Position Pos { get; set; } = new();
    PositionServices Services { get; set; } = new();

    public PlaybackService(MediaElement mediaElement)
    {
        this._mediaElement = mediaElement;
        mediaElement.MediaOpened += Slider_DragCompleted;
    }
    ~PlaybackService()
    {
        _mediaElement.MediaOpened -= Slider_DragCompleted;
        _mediaElement.StateChanged -= Media_Stopped;
    }

#nullable enable
    public async void Media_Stopped(object? sender, MediaStateChangedEventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        if ((_mediaElement.CurrentState == MediaElementState.Stopped))
        {
            Pos.SavedPosition = _mediaElement.Position;
            Debug.WriteLine($"Saving: {Pos.Title} at: {Pos.SavedPosition.TotalSeconds}");
            await Services.SaveCurrentPosition(Pos);
        }
        if ((_mediaElement.CurrentState == MediaElementState.Paused))
        {
            Pos.SavedPosition = _mediaElement.Position;

            Debug.WriteLine($"Saving: {Pos.Title} at: {Pos.SavedPosition.TotalSeconds}");
            await Services.SaveCurrentPosition(Pos);
        }
    }
    public async void Slider_DragCompleted(object? sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        Pos.Title = Preferences.Default.Get("New_Url", string.Empty);
        Pos.SavedPosition = TimeSpan.FromSeconds(0.00);
        var result = await GetPosition();
        // _mediaElement.MediaOpened -= Slider_DragCompleted;

        Pos.SavedPosition = result.SavedPosition;

        _mediaElement.SeekTo(Pos.SavedPosition);
        Debug.WriteLine($"Seeking: {Pos.Title} at: {Pos.SavedPosition.TotalSeconds}");
        _mediaElement.StateChanged += Media_Stopped;
    }
#nullable disable
    public Task<Position> GetPosition()
    {
        Position result = new();
        foreach (var item in Services.GetAllPositions().Result)
        {
            Debug.WriteLine($"GetPosition title: {item.Title} at: {item.SavedPosition.TotalSeconds}");
            if (item.Title == Pos.Title)
            {
                Debug.WriteLine($"Found {item.Title} at: {item.SavedPosition.TotalSeconds}");
                result.SavedPosition = item.SavedPosition;
                result.Title = item.Title;
            }
        }
        Debug.WriteLine($"Returning title: {result.Title} at: {result.SavedPosition.TotalSeconds}");
        return Task.FromResult(result);
    }
}
