// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NerdNewsNavigator2.IViews;

namespace NerdNewsNavigator2.View.Mobile;
public partial class MobilePodcastPage : ContentPage, IPodcastPage
{
    private readonly INavigation _navigation;
    public MobilePodcastPage(INavigation navigation)
    {
        InitializeComponent();
        _navigation = navigation;
    }
    private void LivePage(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"{nameof(MobileLivePage)}");
    }
}
