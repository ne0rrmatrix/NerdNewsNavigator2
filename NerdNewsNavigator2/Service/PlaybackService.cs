// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service
{
    public class PlaybackService
    {
        private static System.Timers.Timer s_aTimer;
        private bool _hasStarted = false;
        private readonly MediaElement _mediaElement;
        public Position Pos { get; set; } = new();
        PositionServices Services { get; set; } = new();
        public PlaybackService(MediaElement mediaElement)
        {
            this._mediaElement = mediaElement;
        }
#nullable enable
        public async void Media_Stopped(object sender, MediaStateChangedEventArgs e)
        {
            if ((_mediaElement.CurrentState == MediaElementState.Stopped) && Pos.Title != string.Empty)
            {
                await Services.SaveCurrentPosition(Pos);
            }
            if ((_mediaElement.CurrentState == MediaElementState.Paused) && Pos.Title != string.Empty)
            {
                await Services.SaveCurrentPosition(Pos);
            }
        }
        public void Slider_DragCompleted(object? sender, EventArgs e)
        {
            if (sender != null && Pos.Title != string.Empty && _hasStarted != true)
            {
                _hasStarted = true;
                _mediaElement.SeekTo(Pos.SavedPosition);
            }
        }
        public void OnPositionChanged(object? sender, EventArgs e)
        {
            if (sender != null && _hasStarted && Pos.Title != string.Empty)
            {
                Pos.SavedPosition = _mediaElement.Position;
            }
        }
#nullable disable
        public Task SetTimer()
        {
            s_aTimer = new System.Timers.Timer(2000);
            s_aTimer.Elapsed += OnTimedEvent;
            s_aTimer.Enabled = true;
            return Task.CompletedTask;
        }
        public async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            s_aTimer.Stop();
            s_aTimer.Dispose();

            CancelEventArgs args = new()
            {
                Cancel = true
            };

            Pos.Title = Preferences.Default.Get("New_Url", string.Empty);
            var result = await GetPosition();

            if (result.SavedPosition.TotalSeconds > 0 && Pos.Title != string.Empty)
            {
                Pos.SavedPosition = result.SavedPosition;
            }

            _mediaElement.PositionChanged += Slider_DragCompleted;
        }
        public Task<Position> GetPosition()
        {
            var pos_List = Services.GetCurrentPosition();
            Position result = new();

            if (pos_List != null)
                foreach (var item in pos_List)
                {
                    if (Pos.Title == item.Title && Pos.Title != string.Empty)
                    {
                        result.SavedPosition = item.SavedPosition;
                    }
                }

            //await Services.DeleteAll();
            return Task.FromResult(result);
        }
    }
}
