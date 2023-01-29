// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View.Phone;

public partial class PhoneShowPage : ContentPage
{
    public PhoneShowPage(PhoneShowViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync($"{nameof(PhonePodcastPage)}");
        return true;
    }
    private void LivePage(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"{nameof(PhoneLivePage)}");
    }
    private void GoBack(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"{nameof(PhonePodcastPage)}");
    }
    private void OnQuit(object sender, EventArgs e)
    {
        Application.Current.Quit();
    }
}
