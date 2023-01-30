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

        /*
        Routing.RegisterRoute(nameof(DesktopShowPage), typeof(DesktopShowPage));
        Routing.RegisterRoute(nameof(DesktopPlayPodcastPage), typeof(DesktopPlayPodcastPage));
        Routing.RegisterRoute(nameof(DesktopLivePage), typeof(DesktopLivePage));
        Routing.RegisterRoute(nameof(DesktopPodcastPage), typeof(DesktopPodcastPage));

        Routing.RegisterRoute(nameof(FirstPage), typeof(FirstPage));

        Routing.RegisterRoute(nameof(PhonePodcastPage), typeof(PhonePodcastPage));
        Routing.RegisterRoute(nameof(PhoneShowPage), typeof(PhoneShowPage));
        Routing.RegisterRoute(nameof(PhonePlayPodcastPage), typeof(PhonePlayPodcastPage));
        Routing.RegisterRoute(nameof(PhoneLivePage), typeof(PhoneLivePage));

        Routing.RegisterRoute(nameof(TabletPlayPodcastPage), typeof(TabletPlayPodcastPage));
        Routing.RegisterRoute(nameof(TabletLivePage), typeof(TabletLivePage));
        Routing.RegisterRoute(nameof(TabletPodcastPage), typeof(TabletPodcastPage));
        Routing.RegisterRoute(nameof(TabletShowPage), typeof(TabletShowPage));
        Routing.RegisterRoute(nameof(LivePage), typeof(LivePage));
        */
    }
    void RegisterRoutes()
    {
        Routes.Add(nameof(DesktopShowPage), typeof(DesktopShowPage));
        Routes.Add(nameof(DesktopPlayPodcastPage), typeof(DesktopPlayPodcastPage));
        Routes.Add(nameof(DesktopLivePage), typeof(DesktopLivePage));
        Routes.Add(nameof(DesktopPodcastPage), typeof(DesktopPodcastPage));

        Routes.Add(nameof(FirstPage), typeof(FirstPage));
        Routes.Add(nameof(FirstVieModel), typeof(FirstVieModel));
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

        System.Diagnostics.Debug.WriteLine("the Next page is: " + GetRoute());
        if (GetRoute() == "Desktop")
            Shell.Current.GoToAsync($"{nameof(DesktopPodcastPage)}");
        else if (GetRoute() == "Phone")
            Shell.Current.GoToAsync($"{nameof(PhonePlayPodcastPage)}");
        else if (GetRoute() == "Tablet")
            Shell.Current.GoToAsync($"{nameof(TabletPlayPodcastPage)}");
        else Shell.Current.GoToAsync($"{nameof(DesktopPodcastPage)}");
    }
    public string GetRoute()
    {
        string device = string.Empty;
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI) { device = "Desktop"; }
        if ((DeviceInfo.Current.Idiom == DeviceIdiom.Tablet) && (DeviceInfo.Current.Platform != DevicePlatform.WinUI))
            device = "Tablet";
        if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
            device = "Phone";

        return device;
    }
}
