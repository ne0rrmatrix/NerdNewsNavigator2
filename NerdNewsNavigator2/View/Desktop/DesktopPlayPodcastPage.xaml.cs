// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Xml.Linq;

namespace NerdNewsNavigator2.View.Desktop;

public partial class DesktopPlayPodcastPage : ContentPage, IPlayPodcastPage
{
    public DesktopPlayPodcastPage(string url)
    {
        InitializeComponent();

        this.BindingContext = new DesktopPlayPodcastViewModel(url);
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
}
