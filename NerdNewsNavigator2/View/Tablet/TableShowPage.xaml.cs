// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View.Tablet;

public partial class TabletShowPage : ContentPage
{
    public TabletShowPage(TabletShowViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync($"{nameof(TabletPodcastPage)}");
        return true;
    }
}
