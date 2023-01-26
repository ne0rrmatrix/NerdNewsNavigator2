// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View.Desktop;

public partial class DesktopPodcastPage : ContentPage, IPodcastPage
{
    public DesktopPodcastPage()
    {

        InitializeComponent();
        this.BindingContext = new DesktopPodcastViewModel(this.Navigation);
    }
    private async void LivePage(object sender, EventArgs e)
    {
        _ = Shell.Current.GoToAsync($"{nameof(DesktopLivePage)}");
    }
}
