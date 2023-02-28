// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that Manages the display of the most recent Shows from twit.tv
/// </summary>
public partial class MostRecentShowsPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of <see cref="MostRecentShowsPage"/>
    /// </summary>
    /// <param name="viewmodel"></param>
    public MostRecentShowsPage(MostRecentShowsViewModel viewmodel)
    {
        InitializeComponent();
        BindingContext = viewmodel;
    }
    private async void Button_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Ok", "Added show to downloads!", "Ok");
    }
}
