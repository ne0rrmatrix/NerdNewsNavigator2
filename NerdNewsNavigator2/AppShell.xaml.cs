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
        Routes.Add(nameof(DesktopShowPage), typeof(DesktopShowPage));
        Routes.Add(nameof(DesktopPlayPodcastPage), typeof(DesktopPlayPodcastPage));
        Routes.Add(nameof(DesktopLivePage), typeof(DesktopLivePage));
        Routes.Add(nameof(DesktopPodcastPage), typeof(DesktopPodcastPage));

        Routes.Add(nameof(FirstPage), typeof(FirstPage));
        Routes.Add(nameof(FirstVieModel), typeof(FirstVieModel));

        Routes.Add(nameof(SettingsPage), typeof(SettingsPage));
        Routes.Add(nameof(SettingsViewModel), typeof(SettingsViewModel));

        Routes.Add(nameof(PhonePodcastPage), typeof(PhonePodcastPage));
        Routes.Add(nameof(PhoneShowPage), typeof(PhoneShowPage));
        Routes.Add(nameof(PhonePlayPodcastPage), typeof(PhonePlayPodcastPage));
        Routes.Add(nameof(PhoneLivePage), typeof(PhoneLivePage));

        Routes.Add(nameof(TabletPlayPodcastPage), typeof(TabletPlayPodcastPage));
        Routes.Add(nameof(TabletLivePage), typeof(TabletLivePage));
        Routes.Add(nameof(TabletPodcastPage), typeof(TabletPodcastPage));
        Routes.Add(nameof(TabletShowPage), typeof(TabletShowPage));

        Routes.Add(nameof(LivePage), typeof(LivePage));

        foreach (var item in Routes)
        {
            Routing.RegisterRoute(item.Key, item.Value);
        }
    }
    private void Quit(object sender, EventArgs e) => Application.Current.Quit();
    private void GotoFirstPage(object sender, EventArgs e)
    {
        string _deviceType = GetRoute();

        if (GetRoute() == "Desktop")
            Shell.Current.GoToAsync($"{nameof(DesktopPodcastPage)}");
        else if (GetRoute() == "Phone")
            Shell.Current.GoToAsync($"{nameof(PhonePodcastPage)}");
        else if (GetRoute() == "Tablet")
            Shell.Current.GoToAsync($"{nameof(TabletPodcastPage)}");
    }
    private async void Reset(object sender, EventArgs e)
    {
        PodcastServices podcastServices = new PodcastServices();
        await podcastServices.DeleteAll();
    }
    public string GetRoute()
    {
        string device = string.Empty;
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI) { device = "Desktop"; }
        else if ((DeviceInfo.Current.Idiom == DeviceIdiom.Tablet) && (DeviceInfo.Current.Platform != DevicePlatform.WinUI))
            device = "Tablet";
        else if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
            device = "Phone";

        return device;
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
