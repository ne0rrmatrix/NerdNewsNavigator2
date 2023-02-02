// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Timers;

namespace NerdNewsNavigator2.View.Desktop;
public partial class DesktopPlayPodcastPage : ContentPage
{
    private static System.Timers.Timer aTimer;
    private bool Timing { get; set; }
    public List<Position> Pos_List { get; set; } = new();
    public Position Pos { get; set; } = new();
    PositionServices services { get; set; } = new();
    string Url;
    public DesktopPlayPodcastPage(DesktopPlayPodcastViewModel viewmodel)
    {
        InitializeComponent();
        BindingContext = viewmodel;
        SetTimer();
    }
#nullable enable
    void Media_Stopped(object sender, MediaStateChangedEventArgs e)
    {
        PositionServices services = new();
        if (mediaElement.CurrentState == MediaElementState.Stopped)
        {
            System.Diagnostics.Debug.WriteLine("Media has Stopped!");
            System.Diagnostics.Debug.WriteLine("Url is: " + Pos.Title + " Current Position: " + Pos.SavedPosition.TotalSeconds);
            services.SaveCurrentPosition(Pos);
        }
        if (mediaElement.CurrentState == MediaElementState.Paused)
        {
            System.Diagnostics.Debug.WriteLine("Media has Paused!");
            System.Diagnostics.Debug.WriteLine("Url is: " + Pos.Title + " Current Position: " + Pos.SavedPosition.TotalSeconds);
            services.SaveCurrentPosition(Pos);
        }
    }
    void Slider_DragCompleted(object? sender, EventArgs e)
    {
        if (sender != null)
        {
            mediaElement.SeekTo(Pos.SavedPosition);
        }
    }
    void OnPositionChanged(object? sender, EventArgs e)
    {
        if (sender != null)
        {
            Pos.SavedPosition = mediaElement.Position;
            System.Diagnostics.Debug.WriteLine("Desktop Page On Position changed is: " + Pos.SavedPosition.TotalSeconds);
        }
    }
#nullable disable
    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync($"{nameof(DesktopPodcastPage)}");
        return true;
    }
    private void SetTimer()
    {
        // Create a timer with a two second interval.
        aTimer = new System.Timers.Timer(3000);
        // Hook up the Elapsed event for the timer. 
        aTimer.Elapsed += OnTimedEvent;
        //  aTimer.AutoReset = true;
        aTimer.Enabled = true;
    }
    private void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
                          e.SignalTime);
        Timing = true;
        aTimer.Stop();
        aTimer.Dispose();
        Url = Preferences.Default.Get("New_Url", "Unknown");
        GetPosition();
        mediaElement.MediaOpened += Slider_DragCompleted;
        OnPropertyChanged(nameof(mediaElement));
        mediaElement.StateChanged += Media_Stopped;
        mediaElement.PositionChanged += OnPositionChanged;
    }
    public void GetPosition()
    {
        SetTimer();
        Pos_List = services.GetCurrentPosition();

        try
        {
            if (Pos_List != null)
                foreach (var item in Pos_List)
                {
                    System.Diagnostics.Debug.WriteLine(item.Title);
                    if (Url == item.Title)
                    {
                        Pos.SavedPosition = item.SavedPosition;
                        Pos.Title = item.Title;
                        System.Diagnostics.Debug.WriteLine("Desktop Podcast page Title is: " + Pos.Title + " " + " Position is: " + Pos.SavedPosition.TotalSeconds);
                        break;
                    }
                }
        }
        catch { }
    }
    //  services.DeleteAll();
}
