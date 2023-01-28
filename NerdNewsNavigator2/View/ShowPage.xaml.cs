// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;
public partial class ShowPage : ContentPage
{
    public ShowPage(ShowViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    private void LivePage(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"{nameof(LivePage)}");
    }
    private void GoBack(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"..");
    }
    private void OnQuit(object sender, EventArgs e)
    {
        Application.Current.Quit();
    }
}
