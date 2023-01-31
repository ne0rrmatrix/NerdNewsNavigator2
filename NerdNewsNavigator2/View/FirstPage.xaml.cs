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
        if (_deviceType == "Desktop")
            Shell.Current.GoToAsync($"{nameof(DesktopPodcastPage)}");
        else if (_deviceType == "Phone")
            Shell.Current.GoToAsync($"{nameof(PhonePodcastPage)}");
        else if (_deviceType == "Tablet")
            Shell.Current.GoToAsync($"{nameof(TabletPodcastPage)}");
    }
    public string GetRoute()
    {
        string device = string.Empty;
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI) { device = "Desktop"; }
        else if ((DeviceInfo.Current.Idiom == DeviceIdiom.Tablet) && (DeviceInfo.Current.Platform != DevicePlatform.WinUI))
            device = "Tablet";
        else if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone && (DeviceInfo.Current.Idiom != DeviceIdiom.Tablet))
            device = "Phone";

        return device;
    }
}
