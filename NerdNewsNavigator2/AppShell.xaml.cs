// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NerdNewsNavigator2.View;

namespace NerdNewsNavigator2;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(TabletPodcastPage), typeof(TabletPodcastPage));
        Routing.RegisterRoute(nameof(TabletShowPage), typeof(TabletShowPage));
        Routing.RegisterRoute(nameof(TabletPlayPodcastPage), typeof(TabletPlayPodcastPage));
        Routing.RegisterRoute(nameof(LivePage), typeof(LivePage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(UpdateSettingsPage), typeof(UpdateSettingsPage));
    }
    private void Quit(object sender, EventArgs e) => Application.Current.Quit();

    private async void GotoFirstPage(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"{nameof(TabletPodcastPage)}");
    }
    private async void Reset(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"{nameof(UpdateSettingsPage)}");
    }
    private async void GotoLivePage(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"{nameof(LivePage)}");
    }
    private async void GotoSettingsPage(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"{nameof(SettingsPage)}");
    }
}
