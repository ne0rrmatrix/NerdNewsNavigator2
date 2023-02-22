// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;
public partial class AddPodcastPage : ContentPage
{
    public AddPodcastPage(AddPodcastViewModel viewModel)
    {

        InitializeComponent();
        BindingContext = viewModel;
    }
    private async void Button_Clicked(object sender, EventArgs e)
    {
        await PodcastServices.AddPodcast(Url.Text.ToString());
        await DisplayAlert("Sucess", "Podcast Added!", "Ok");
    }
    private async void AddDefault(object sender, EventArgs e)
    {
        await PodcastServices.AddDefaultPodcasts();
        await DisplayAlert("Sucess", "Defaults Added!", "Ok");
    }
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
    private async void RemovePodcasts(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(RemovePage)}");
    }

    private async void ResetPodcasts(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(UpdateSettingsPage)}");
    }

    private async void OpenUrlForDonation(object sender, EventArgs e)
    {
        await Browser.OpenAsync("https://www.paypal.com/donate/?business=LYEHGH249KCP2&no_recurring=0&item_name=All+donations+are+welcome.+It+helps+support+development+of+NerdNewsNavigator.+Thank+you+for+your+support.&currency_code=CAD");
    }

    private async void Button_Full_Screen_Disable(object sender, EventArgs e)
    {
        Preferences.Default.Remove("FullScreen", null);
        Preferences.Default.Set("FullScreen", false);
        Debug.WriteLine("Set full screen disabled");
        await DisplayAlert("Sucess", "Full Screen Disabled", "Ok");
    }

    private async void Button_Full_Screen_Enable(object sender, EventArgs e)
    {
        Preferences.Default.Remove("FullScreen", null);
        Preferences.Default.Set("FullScreen", true);
        Debug.WriteLine("Set full screen enabled.");
        await DisplayAlert("Sucess", "Full Screen Enabled", "Ok");
    }
}
