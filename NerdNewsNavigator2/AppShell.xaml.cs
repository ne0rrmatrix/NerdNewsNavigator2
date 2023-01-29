// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.



namespace NerdNewsNavigator2;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(DesktopShowPage), typeof(DesktopShowPage));
        Routing.RegisterRoute(nameof(DesktopPlayPodcastPage), typeof(DesktopPlayPodcastPage));
        Routing.RegisterRoute(nameof(DesktopLivePage), typeof(DesktopLivePage));
        Routing.RegisterRoute(nameof(DesktopPodcastPage), typeof(DesktopPodcastPage));

        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));

        Routing.RegisterRoute(nameof(PhonePodcastPage), typeof(PhonePodcastPage));
        Routing.RegisterRoute(nameof(PhoneShowPage), typeof(PhoneShowPage));
        Routing.RegisterRoute(nameof(PhonePlayPodcastPage), typeof(PhonePlayPodcastPage));
        Routing.RegisterRoute(nameof(PhoneLivePage), typeof(PhoneLivePage));

        Routing.RegisterRoute(nameof(TabletPlayPodcastPage), typeof(TabletPlayPodcastPage));
        Routing.RegisterRoute(nameof(TabletLivePage), typeof(TabletLivePage));
        Routing.RegisterRoute(nameof(TabletPodcastPage), typeof(TabletPodcastPage));
        Routing.RegisterRoute(nameof(TabletShowPage), typeof(TabletShowPage));
    }
}
