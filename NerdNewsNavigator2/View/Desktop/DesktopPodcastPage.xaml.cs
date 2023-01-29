// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View.Desktop;
public partial class DesktopPodcastPage : ContentPage
{
    public DesktopPodcastPage(DesktopPodcastViewModel viewmodel)
    {
        InitializeComponent();
        BindingContext = viewmodel;
    }
    private void OnQuit(object sender, EventArgs e)
    {
        Application.Current.Quit();
    }
    private void LivePage(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"{nameof(DesktopLivePage)}");
    }
}
