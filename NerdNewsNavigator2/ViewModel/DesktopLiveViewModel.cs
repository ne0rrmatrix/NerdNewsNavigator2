// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

public partial class DesktopLiveViewModel : ObservableObject, IPlayLivePage
{
    readonly INavigation _navigation;
    [ObservableProperty]
    public string _url = "https://www.youtube.com/embed/yQPlcthGEe4?autoplay=1";

    public DesktopLiveViewModel(INavigation navigation)
    {
        this._navigation = navigation;
    }
    public DesktopLiveViewModel(string url)
    {

    }
}
