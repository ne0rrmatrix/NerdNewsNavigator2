// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that Manages displaying Podcasts from <see cref="PodcastServices"/>
/// </summary>
public partial class SettingsPage : ContentPage
{

    /// <summary>
    /// Private <see cref="bool"/> which sets Full Screen Mode.
    /// </summary>
    private bool FullScreenMode { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of <see cref="SettingsPage"/>
    /// </summary>
    /// <param name="viewModel">The <see cref="ViewModel"/> instance that is managed through this class.</param> 
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>
    /// The Method controls Adding a <see cref="Podcast"/> to <see cref="List{T}"/> of class <see cref="Podcast"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (sender is null || Url.Text is null)
        {
            return;
        }
        await PodcastServices.AddPodcast(Url.Text.ToString());
        await Toast.Make("Podcast Added!.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        await Shell.Current.GoToAsync($"{nameof(SettingsPage)}");
    }

    /// <summary>
    /// The Method controls adding all default <see cref="Podcast"/> to <see cref="List{T}"/> of class <see cref="Podcast"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddDefault(object sender, EventArgs e)
    {
        await PodcastServices.AddDefaultPodcasts();
        await Toast.Make("Defaults Added!.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
    }

    /// <summary>
    /// The Method controls Removing Default <see cref="Podcast"/> from <see cref="List{T}"/> of class <see cref="Podcast"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveDefault(object sender, EventArgs e)
    {
        var item = await App.PositionData.GetAllPodcasts();
        if (item.AsEnumerable().Any(x => !x.Url.Contains("feeds.twit.tv")))
        {
            await PodcastServices.RemoveDefaultPodcasts();
            await Toast.Make("Defaults removed.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        }
        else
        {
            await DisplayAlert("", "At least one podcast needs to be added", "Ok");
        }
    }

    /// <summary>
    /// The Method manages Removing a <see cref="Podcast"/> from <see cref="List{T}"/> of class <see cref="Podcast"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void EditPodcasts(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(EditPage)}");
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
        var item = "https://www.paypal.com/donate/?business=LYEHGH249KCP2&no_recurring=0&item_name=All+donations+are+welcome.+It+helps+support+development+of+NerdNewsNavigator.+Thank+you+for+your+support.&currency_code=CAD";
        await Browser.OpenAsync(item);
    }

    /// <summary>
    /// Method sets screen to normal screen size.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        DeviceService.RestoreScreen();
    }
}
