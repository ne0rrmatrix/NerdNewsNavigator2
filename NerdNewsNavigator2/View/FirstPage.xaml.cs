// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;
public partial class FirstPage : ContentPage
{
    public FirstPage(FirstVieModel vieModel)
    {
        InitializeComponent();
        BindingContext = vieModel;

        var deviceType = FirstPage.GetRoute();

        if (deviceType == "Desktop")
            Shell.Current.GoToAsync($"{nameof(DesktopPodcastPage)}");
        else if (deviceType == "Phone")
            Shell.Current.GoToAsync($"{nameof(PhonePodcastPage)}");
        else if (deviceType == "Tablet")
            Shell.Current.GoToAsync($"{nameof(TabletPodcastPage)}");
    }
    public static string GetRoute()
    {
        var device = string.Empty;
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI) { device = "Desktop"; }
        else if ((DeviceInfo.Current.Idiom == DeviceIdiom.Tablet) && (DeviceInfo.Current.Platform != DevicePlatform.WinUI))
            device = "Tablet";
        else if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone && (DeviceInfo.Current.Idiom != DeviceIdiom.Tablet))
            device = "Phone";

        return device;
    }
}
