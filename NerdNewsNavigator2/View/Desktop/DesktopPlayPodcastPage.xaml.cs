// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View.Desktop;
public partial class DesktopPlayPodcastPage : ContentPage
{
    public Position Position { get; set; } = new();
    public DesktopPlayPodcastPage(DesktopPlayPodcastViewModel viewmodel)
    {
        InitializeComponent();
        BindingContext = viewmodel;
        GetPosition();
        System.Diagnostics.Debug.WriteLine("Title is: " + Position.Title + " " + " Position is: " + Position.SavedPosition);
        mediaElement.MediaOpened += Slider_DragCompleted;
        mediaElement.StateChanged += Media_Stopped;
        OnPropertyChanged(nameof(mediaElement));
        mediaElement.PositionChanged += OnPositionChanged;
    }
#nullable enable
    void Media_Stopped(object sender, MediaStateChangedEventArgs e)
    {
        PositionServices services = new();
        if (mediaElement.CurrentState == MediaElementState.Stopped)
        {
            System.Diagnostics.Debug.WriteLine("Media has Stopped!");
            services.SaveCurrentPosition(Position);
        }
        if (mediaElement.CurrentState == MediaElementState.Paused)
        {
            System.Diagnostics.Debug.WriteLine("Media has Paused!");
            services.SaveCurrentPosition(Position);
        }
    }
    void Slider_DragCompleted(object? sender, EventArgs e)
    {
        if (sender != null)
        {
            mediaElement.SeekTo(Position.SavedPosition);
        }
    }
    void OnPositionChanged(object? sender, EventArgs e)
    {
        if (sender != null)
        {
            Position.SavedPosition = mediaElement.Position;
            System.Diagnostics.Debug.WriteLine("Position changed to position: " + Position.SavedPosition);
        }
    }
#nullable disable
    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync($"{nameof(DesktopPodcastPage)}");
        return true;
    }
    public void GetPosition()
    {
        PositionServices services = new();
        // Position.SavedPosition = TimeSpan.FromSeconds(200.00);
        //return Position;
        if (services.GetCurrentPosition() != null)
            Position = services.GetCurrentPosition();
    }

    /*
#nullable enable
    void MediaElement_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == MediaElement.DurationProperty.PropertyName)
        {
            logger.LogInformation("Duration: {newDuration}", mediaElement.Duration);
            positionSlider.Maximum = mediaElement.Duration.TotalSeconds;
        }
    }

    void OnMediaOpened(object? sender, EventArgs e) => logger.LogInformation("Media opened.");

    void OnStateChanged(object? sender, MediaStateChangedEventArgs e) =>
        logger.LogInformation("Media State Changed. Old State: {PreviousState}, New State: {NewState}", e.PreviousState, e.NewState);

    void OnMediaFailed(object? sender, MediaFailedEventArgs e) => logger.LogInformation("Media failed. Error: {ErrorMessage}", e.ErrorMessage);

    void OnMediaEnded(object? sender, EventArgs e) => logger.LogInformation("Media ended.");

    void OnPositionChanged(object? sender, MediaPositionChangedEventArgs e)
    {
        logger.LogInformation("Position changed to {position}", e.Position);
        positionSlider.Value = e.Position.TotalSeconds;
    }

    void OnSeekCompleted(object? sender, EventArgs e) => logger.LogInformation("Seek completed.");

    void OnSpeedMinusClicked(object? sender, EventArgs e)
    {
        if (mediaElement.Speed >= 1)
        {
            mediaElement.Speed -= 1;
        }
    }

    void OnSpeedPlusClicked(object? sender, EventArgs e)
    {
        if (mediaElement.Speed < 10)
        {
            mediaElement.Speed += 1;
        }
    }

    void OnVolumeMinusClicked(object? sender, EventArgs e)
    {
        if (mediaElement.Volume >= 0)
        {
            if (mediaElement.Volume < .1)
            {
                mediaElement.Volume = 0;

                return;
            }

            mediaElement.Volume -= .1;
        }
    }

    void OnVolumePlusClicked(object? sender, EventArgs e)
    {
        if (mediaElement.Volume < 1)
        {
            if (mediaElement.Volume > .9)
            {
                mediaElement.Volume = 1;

                return;
            }

            mediaElement.Volume += .1;
        }
    }

    void OnPlayClicked(object? sender, EventArgs e)
    {
        mediaElement.Play();
    }

    void OnPauseClicked(object? sender, EventArgs e)
    {
        mediaElement.Pause();
    }

    void OnStopClicked(object? sender, EventArgs e)
    {
        mediaElement.Stop();
    }

    void BasePage_Unloaded(object? sender, EventArgs e)
    {
        // Stop and cleanup MediaElement when we navigate away
        mediaElement.Handler?.DisconnectHandler();
    }

    void Slider_DragCompleted(object? sender, EventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);

        var newValue = ((Slider)sender).Value;
        mediaElement.SeekTo(TimeSpan.FromSeconds(newValue));
        mediaElement.Play();
    }

    void Slider_DragStarted(object sender, EventArgs e)
    {
        mediaElement.Pause();
    }

    async void ChangeAspectClicked(System.Object sender, System.EventArgs e)
    {
        var resultAspect = await DisplayActionSheet("Choose aspect ratio",
            "Cancel", null, Aspect.AspectFit.ToString(),
            Aspect.AspectFill.ToString(), Aspect.Fill.ToString());

        if (resultAspect.Equals("Cancel"))
        {
            return;
        }

        if (!Enum.TryParse(typeof(Aspect), resultAspect, true, out var aspectEnum)
            || aspectEnum is null)
        {
            await DisplayAlert("Error", "There was an error determining the selected aspect",
                "OK");

            return;
        }

        mediaElement.Aspect = (Aspect)aspectEnum;
    }
#nullable disable
    */
}
