// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace NerdNewsNavigator2.ViewModel;
[QueryProperty("Url", "Url")]
public partial class DesktopPlayPodcastViewModel : ObservableObject, IPlayPodcastPage
{
    #region Properties
    private readonly INavigation _navigation;
    [ObservableProperty]
    private string _url;
    #endregion
    public DesktopPlayPodcastViewModel(INavigation navigation)
    {
        this._navigation = navigation;
    }
    public DesktopPlayPodcastViewModel(string Url)
    {
        System.Diagnostics.Debug.WriteLine("String in PlayPodcast is: " + Url);
        this.Url = HttpUtility.UrlDecode(Url);
    }
}
