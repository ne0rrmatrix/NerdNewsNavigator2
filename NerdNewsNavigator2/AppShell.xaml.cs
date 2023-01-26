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
        Routing.RegisterRoute(nameof(MobileShowPage), typeof(MobileShowPage));

        Routing.RegisterRoute(nameof(DesktopPlayPodcastPage), typeof(DesktopPlayPodcastPage));
        Routing.RegisterRoute(nameof(MobilePlayPodcastPage), typeof(MobilePlayPodcastPage));

        Routing.RegisterRoute(nameof(DesktopLivePage), typeof(DesktopLivePage));
        Routing.RegisterRoute(nameof(MobileLivePage), typeof(MobileLivePage));
    }
}
