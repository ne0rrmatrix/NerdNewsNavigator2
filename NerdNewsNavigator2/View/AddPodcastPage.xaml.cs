// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;
public partial class AddPodcastPage : ContentPage
{
    PodcastServices PodServices { get; set; } = new();
    public AddPodcastPage(AddPodcastViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            await PodServices.AddPodcast(Url.Text.ToString());
            await DisplayAlert("Sucess", "Podcast Added!", "Ok");
        }
        catch
        {
        }
    }
    private async void RadioButton_Item(object sender, CheckedChangedEventArgs e)
    {
        var button = sender as RadioButton;
        if (button.Content.ToString() == "Enabled")
        {
            try
            {
                await PodServices.AddDefaultPodcasts();
                await DisplayAlert("Sucess", "Defaults Added!", "Ok");
            }
            catch { }
        }
        if (button.Content.ToString() == "Disabled")
        {
            try
            {
                var temp = true;
                var item = PodServices.GetAllPodcasts().Result;
                foreach (var podcast in item)
                {
                    if (!podcast.Url.Contains("feeds.twit.tv"))
                    {
                        temp = false;
                    }
                }
                if (!temp)
                {
                    await PodServices.RemoveDefaultPodcasts();
                    await DisplayAlert("Sucess", "Defaults removed", "Ok");
                }
                else if (temp)
                {
                    await DisplayAlert("Failed", "At least one podcast needs to be added", "Ok");
                }

            }
            catch { }
        }
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(RemovePage)}");
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(UpdateSettingsPage)}");
    }
}
