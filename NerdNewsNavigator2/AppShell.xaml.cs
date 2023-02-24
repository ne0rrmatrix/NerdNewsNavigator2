// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2;
/// <summary>
/// A class that inherits <see cref="Shell"/> and manages it.
/// </summary>
public partial class AppShell : Shell
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppShell"/> class.
    /// </summary>
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(TabletPodcastPage), typeof(TabletPodcastPage));
        Routing.RegisterRoute(nameof(TabletShowPage), typeof(TabletShowPage));
        Routing.RegisterRoute(nameof(TabletPlayPodcastPage), typeof(TabletPlayPodcastPage));
        Routing.RegisterRoute(nameof(LivePage), typeof(LivePage));
        Routing.RegisterRoute(nameof(RemovePage), typeof(RemovePage));
        Routing.RegisterRoute(nameof(UpdateSettingsPage), typeof(UpdateSettingsPage));
        Routing.RegisterRoute(nameof(AddPodcastPage), typeof(AddPodcastPage));
        Routing.RegisterRoute(nameof(MostRecentShowsPage), typeof(MostRecentShowsPage));
    }
    /// <summary>
    /// Method quits application
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Quit(object sender, EventArgs e) => Application.Current.Quit();

    /// <summary>
    /// Method navigates user to Main Page.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GotoFirstPage(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"{nameof(TabletPodcastPage)}");
    }

    /// <summary>
    /// Method navigates user to Main Page.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GotoMostRecentShowPage(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"{nameof(MostRecentShowsPage)}");
    }

    /// <summary>
    /// Method resets database.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Reset(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(UpdateSettingsPage)}");
    }

    /// <summary>
    /// Method navigates user to Live Video for twit.tv
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GotoLivePage(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"{nameof(LivePage)}");
    }

    /// <summary>
    /// Method navigates user to Page that allows you to add a podcast.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GotoAddPage(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"{nameof(AddPodcastPage)}");
    }
}
