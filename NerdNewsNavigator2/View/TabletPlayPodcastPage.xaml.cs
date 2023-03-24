// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
#endif

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that Displays a Video from twit.tv.
/// </summary>
/// 
public partial class TabletPlayPodcastPage : ContentPage
{
    #region Properties

    /// <summary>
    /// Initilizes a new instance of the <see cref="ILogger{TCategoryName}"/> class
    /// </summary>
    private readonly ILogger<TabletPlayPodcastPage> _logger;

    /// <summary>
    /// Initilizes a new instance of the <see cref="Position"/> class
    /// </summary>
    private Position Pos { get; set; } = new();

    #endregion

    /// <summary>
    /// Class Constructor that initilizes <see cref="TabletPlayPodcastPage"/>
    /// </summary>
    /// <param name="logger">This Applications <see cref="ILogger{TCategoryName}"/> instance is managed through this class</param>
    /// <param name="viewModel">This Applications <see cref="TabletPlayPodcastPage"/> instance is managed through this class.</param>

    public TabletPlayPodcastPage(ILogger<TabletPlayPodcastPage> logger, TabletPlayPodcastViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _logger = logger;

#if WINDOWS || ANDROID
        mediaElement.MediaOpened += Seek;
#endif

#if IOS
        mediaElement.StateChanged += SeekIOS;
#endif
    }
    /// <summary>
    /// Method overrides <see cref="OnDisappearing"/> to stop playback when leaving a page.
    /// </summary>
    protected override void OnDisappearing()
    {
        mediaElement.Stop();
        mediaElement.ShouldKeepScreenOn = false;
    }

#nullable enable

    /// <summary>
    /// Manages IOS seeking for <see cref="mediaElement"/> with <see cref="Pos"/> at start of playback.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public async void SeekIOS(object? sender, MediaStateChangedEventArgs e)
    {
        if (sender == null)
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
                _logger.LogInformation("Retrieved Saved position from database is: {Title} - {TotalSeconds}", item.Title, item.SavedPosition);
            }
        }
        if (e.NewState == MediaElementState.Playing)
        {
            mediaElement.SeekTo(Pos.SavedPosition);
            mediaElement.ShouldKeepScreenOn = true;
            _logger.LogInformation("Media playback started. ShouldKeepScreenOn is set to true.");
        }
    }

    /// <summary>
    /// Manages the saving of <see cref="Position"/> data in <see cref="PositionDataBase"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public async void Media_Stopped(object? sender, MediaStateChangedEventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        switch (e.NewState)
        {
            case MediaElementState.Playing:
                SetFullScreen();
                break;

            case MediaElementState.Stopped:
                _logger.LogInformation("Media has finished playing.");
                mediaElement.ShouldKeepScreenOn = false;
                _logger.LogInformation("ShouldKeepScreenOn set to false.");
                break;

            case MediaElementState.Paused:
                if (mediaElement.Position > Pos.SavedPosition)
                {
                    Pos.SavedPosition = mediaElement.Position;
                    _logger.LogInformation("Paused: {Position}", mediaElement.Position);
                    await Save();
                }
                break;
        }
        switch (e.PreviousState)
        {
            case MediaElementState.Playing:
                if (mediaElement.Position < Pos.SavedPosition)
                {
                    Pos.SavedPosition = mediaElement.Position;
                    _logger.LogInformation("Finished Seeking: {Position}", mediaElement.Position);
                    await Save();
                }
                break;
        }
    }

    /// <summary>
    /// Manages <see cref="mediaElement"/> seeking of <see cref="Position"/> at start of playback.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public async void Seek(object? sender, EventArgs e)
    {
        if (sender is null)
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
                _logger.LogInformation("Retrieved Saved position from database is: {Title} - {TotalSeconds}", item.Title, item.SavedPosition);
            }
        }

        mediaElement.ShouldKeepScreenOn = true;
        _logger.LogInformation("Media playback started. ShouldKeepScreenOn is set to true.");
        mediaElement.SeekTo(Pos.SavedPosition);
        mediaElement.StateChanged += Media_Stopped;
    }

#nullable disable

    /// <summary>
    /// Manages saving of <see cref="Pos"/> to <see cref="PositionDataBase"/> Database.
    /// </summary>
    /// <returns></returns>
    public async Task Save()
    {
        var items = await App.PositionData.GetAllPositions();
        foreach (var item in items)
        {
            if (item.Title == Pos.Title)
            {
                await App.PositionData.Delete(item);
            }
        }
        await App.PositionData.Add(new Position
        {
            Title = Pos.Title,
            SavedPosition = Pos.SavedPosition,
        });
    }

    #region Full/Restore Screen Events

#if WINDOWS
    /// <summary>
    /// Method is required for switching Full Screen Mode for Windows
    /// </summary>
    private static Microsoft.UI.Windowing.AppWindow GetAppWindow(MauiWinUIWindow window)
    {
        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
        return appWindow;
    }
#endif

#nullable enable
    /// <summary>
    /// Method toggles Full Screen Off
    /// </summary>
    public void RestoreScreen()
    {
#if WINDOWS
        var window = GetParentWindow().Handler.PlatformView as MauiWinUIWindow;
        if (window is not null)
        {
            var appWindow = GetAppWindow(window);

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
    }

    /// <summary>
    /// Method toggles Full Screen On
    /// </summary>
    private void SetFullScreen()
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
        var window = GetParentWindow().Handler.PlatformView as MauiWinUIWindow;
        if (window is not null)
        {
            var appWindow = GetAppWindow(window);
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
    #endregion

    #region Load/Unload Events
#nullable enable

    /// <summary>
    /// Manages unload event from <see cref="mediaElement"/> after it is unloaded.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void ContentPage_Unloaded(object? sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        // Stop and cleanup MediaElement when we navigate away
        _logger.LogInformation("Unloaded Media Element from memory.");
        mediaElement.StateChanged -= Media_Stopped;
        Pos.SavedPosition = TimeSpan.Zero;
        Pos.Title = string.Empty;
        mediaElement.Handler?.DisconnectHandler();
    }
    private void ContentPage_Loaded(object? sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        SetFullScreen();
    }
#nullable disable

    #endregion
}
