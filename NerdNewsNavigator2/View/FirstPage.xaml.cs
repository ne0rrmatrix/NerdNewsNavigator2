// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace NerdNewsNavigator2.View;
public partial class FirstPage : ContentPage
{
    public FirstPage(FirstVieModel vieModel)
    {
        InitializeComponent();
        BindingContext = vieModel;

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
