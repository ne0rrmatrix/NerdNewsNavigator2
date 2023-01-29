// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View.Desktop;
public partial class DesktopPlayPodcastPage : ContentPage
{
    public DesktopPlayPodcastPage(DesktopPlayPodcastViewModel viewmodel)
    {
        InitializeComponent();
        BindingContext = viewmodel;
    }
    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync($"{nameof(DesktopPodcastPage)}");
        return true;
    }
}
