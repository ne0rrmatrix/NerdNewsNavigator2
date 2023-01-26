// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace NerdNewsNavigator2.View.Desktop;

public partial class DesktopShowPage : ContentPage, IShowPage
{
    public DesktopShowPage(string url)
    {
        InitializeComponent();
        this.BindingContext = new DesktopShowViewModel(url);
    }
    private void SwipedGesture(object sender, SwipedEventArgs e)
    {
        switch (e.Direction)
        {
            case SwipeDirection.Right:
                Shell.Current.GoToAsync("..");
                break;
        }
    }
    private void LivePage(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"{nameof(MobileLivePage)}");
    }
    private void PodcastPage(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"{nameof(MobilePodcastPage)}");
    }
}
