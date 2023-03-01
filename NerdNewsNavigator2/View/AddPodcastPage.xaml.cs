// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application = Microsoft.Maui.Controls.Application;
using Platform = Microsoft.Maui.ApplicationModel.Platform;
using MetroLog.Maui;

#if ANDROID
using Views = AndroidX.Core.View;
#endif

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that Manages displaying Podcasts from <see cref="PodcastServices"/>
/// </summary>
public partial class AddPodcastPage : ContentPage
{

    /// <summary>
    /// Private <see cref="bool"/> which sets Full Screen Mode.
    /// </summary>
    private bool FullScreenMode { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of <see cref="AddPodcastPage"/>
    /// </summary>
    /// <param name="viewModel">The <see cref="ViewModel"/> instance that is managed through this class.</param> 
    public AddPodcastPage(AddPodcastViewModel viewModel)
    {

        InitializeComponent();
        BindingContext = viewModel;
        //NOTE: Change this to fetch the value true/false according to your app logic.
    }

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

    /// <summary>
    /// The Method controls Adding a <see cref="Podcast"/> to <see cref="List{T}"/> of class <see cref="Podcast"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_Clicked(object sender, EventArgs e)
    {
        await PodcastServices.AddPodcast(Url.Text.ToString());
        await DisplayAlert("Sucess", "Podcast Added!", "Ok");
    }

    /// <summary>
    /// The Method controls adding all default <see cref="Podcast"/> to <see cref="List{T}"/> of class <see cref="Podcast"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddDefault(object sender, EventArgs e)
    {
        await PodcastServices.AddDefaultPodcasts();
        await DisplayAlert("Sucess", "Defaults Added!", "Ok");
    }

    /// <summary>
    /// The Method controls Removing Default <see cref="Podcast"/> from <see cref="List{T}"/> of class <see cref="Podcast"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveDefault(object sender, EventArgs e)
    {
        var unique = false;
        var item = await App.PositionData.GetAllPodcasts();
        foreach (var podcast in item)
        {
            if (!podcast.Url.Contains("feeds.twit.tv"))
            {
                unique = true;
            }
        }
        if (!unique)
        {
            await DisplayAlert("Failed", "At least one podcast needs to be added", "Ok");
        }
        else
        {
            await PodcastServices.RemoveDefaultPodcasts();
            await DisplayAlert("Sucess", "Defaults removed", "Ok");

        }
    }

    /// <summary>
    /// The Method manages Removing a <see cref="Podcast"/> from <see cref="List{T}"/> of class <see cref="Podcast"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemovePodcasts(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(RemovePage)}");
    }

    /// <summary>
    /// The Method resets <see cref="List{T}"/> of class <see cref="Podcast"/> to defaults.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>

    private async void ResetPodcasts(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(UpdateSettingsPage)}");
    }

    /// <summary>
    /// The Method opens donation url in default browser
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OpenUrlForDonation(object sender, EventArgs e)
    {
        await Browser.OpenAsync("https://www.paypal.com/donate/?business=LYEHGH249KCP2&no_recurring=0&item_name=All+donations+are+welcome.+It+helps+support+development+of+NerdNewsNavigator.+Thank+you+for+your+support.&currency_code=CAD");
    }

    /// <summary>
    /// The Method disables Full screen on device.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_Full_Screen_Disable(object sender, EventArgs e)
    {
        Preferences.Default.Remove("FullScreen", null);
        Preferences.Default.Set("FullScreen", false);
        FullScreenMode = false;
        SetFullScreen();
        await DisplayAlert("Sucess", "Full Screen Disabled", "Ok");
    }

    /// <summary>
    /// The Method enables full screen on device.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_Full_Screen_Enable(object sender, EventArgs e)
    {
        Preferences.Default.Remove("FullScreen", null);
        Preferences.Default.Set("FullScreen", true);
        FullScreenMode = true;
        SetFullScreen();
        await DisplayAlert("Sucess", "Full Screen Enabled", "Ok");
    }

    /// <summary>
    /// Event triggers the <see cref="SetFullScreen"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
#nullable enable
    private void SetFullScreenItem(object sender, EventArgs e)
    {
        if (FullScreenMode)
        {
            FullScreenMode = false;
            SetFullScreen();
        }
        else
        {
            FullScreenMode = true;
            SetFullScreen();
        }
    }

    /// <summary>
    /// Method toggles Full Screen Mode Off and On
    /// </summary>
    private void SetFullScreen()
    {
#if ANDROID
            var activity = Platform.CurrentActivity;

            if (activity == null || activity.Window == null) return;

            Views.WindowCompat.SetDecorFitsSystemWindows(activity.Window, !FullScreenMode);
            var windowInsetsControllerCompat = Views.WindowCompat.GetInsetsController(activity.Window, activity.Window.DecorView);
            var types = Views.WindowInsetsCompat.Type.StatusBars() |
                        Views.WindowInsetsCompat.Type.NavigationBars();
            if (FullScreenMode)
            {
                windowInsetsControllerCompat.SystemBarsBehavior = Views.WindowInsetsControllerCompat.BehaviorShowBarsBySwipe;
                windowInsetsControllerCompat.Hide(types);
            }
            else
            {
                windowInsetsControllerCompat.Show(types);
            }
#endif
#if WINDOWS
        var window = GetParentWindow().Handler.PlatformView as MauiWinUIWindow;

        var appWindow = GetAppWindow(window);

        switch (appWindow.Presenter)
        {
            case Microsoft.UI.Windowing.OverlappedPresenter overlappedPresenter:
                if (overlappedPresenter.State == Microsoft.UI.Windowing.OverlappedPresenterState.Maximized)
                {
                    overlappedPresenter.SetBorderAndTitleBar(true, true);
                    overlappedPresenter.Restore();
                }
                else
                {
                    overlappedPresenter.SetBorderAndTitleBar(false, false);
                    overlappedPresenter.Maximize();
                }

                break;
        }
#endif
    }
#nullable disable
}
