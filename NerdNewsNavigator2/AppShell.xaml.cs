// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2;

public partial class AppShell : Shell
{
    public Dictionary<string, Type> Routes { get; private set; } = new Dictionary<string, Type>();
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }
    void RegisterRoutes()
    {
        Routes.Add(nameof(SettingsPage), typeof(SettingsPage));
        Routes.Add(nameof(SettingsViewModel), typeof(SettingsViewModel));

        Routes.Add(nameof(TabletPodcastPage), typeof(TabletPodcastPage));
        Routes.Add(nameof(TabletPodcastViewModel), typeof(TabletPodcastViewModel));

        Routes.Add(nameof(TabletShowPage), typeof(TabletShowPage));
        Routes.Add(nameof(TabletShowViewModel), typeof(TabletShowViewModel));

        Routes.Add(nameof(TabletPlayPodcastViewModel), typeof(TabletPlayPodcastViewModel));
        Routes.Add(nameof(TabletPlayPodcastPage), typeof(TabletPlayPodcastPage));

        Routes.Add(nameof(TabletLiveViewModel), typeof(TabletLiveViewModel));
        Routes.Add(nameof(TabletLivePage), typeof(TabletLivePage));

        Routes.Add(nameof(LivePage), typeof(LivePage));
        Routes.Add(nameof(LiveViewModel), typeof(LiveViewModel));

        foreach (var item in Routes)
        {
            Routing.RegisterRoute(item.Key, item.Value);
        }
    }
    private void Quit(object sender, EventArgs e) => Application.Current.Quit();

    private void GotoFirstPage(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"{nameof(TabletPodcastPage)}");
    }
    private async void Reset(object sender, EventArgs e)
    {
        PodcastServices podcastServices = new PodcastServices();
        await podcastServices.DeleteAll();
    }
    private void GotoLivePage(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"{nameof(LivePage)}");
    }
    private void GotoSettingsPage(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"{nameof(SettingsPage)}");
    }
}
