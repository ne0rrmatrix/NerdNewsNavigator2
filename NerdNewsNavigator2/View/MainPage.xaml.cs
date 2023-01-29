// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

public partial class MainPage : ContentPage
{
   
    public MainPage()
    {
        InitializeComponent();
        string _deviceType = GetRoute();
        System.Diagnostics.Debug.WriteLine(GetRoute());
        if (GetRoute() == "Desktop")
            Shell.Current.GoToAsync(nameof(PodcastPage));
    }
    public string GetRoute()
    {
        string device = string.Empty;
        if(DeviceInfo.Current.Platform == DevicePlatform.WinUI) { device = "Desktop"; }
        if ((DeviceInfo.Current.Idiom == DeviceIdiom.Tablet) && (DeviceInfo.Current.Platform != DevicePlatform.WinUI))
            device = "Tablet";
        if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
            device = "Phone";

        return device;
    }
}
