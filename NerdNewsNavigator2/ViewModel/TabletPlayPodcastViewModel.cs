// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

[QueryProperty("Url", "Url")]
public partial class TabletPlayPodcastViewModel : BaseViewModel
{
    #region Properties
    string url;
    public string Url
    {
        get => url;
        set
        {
            SetProperty(ref url, value);
            Preferences.Default.Clear();
            Preferences.Default.Set("New_Url", value);
        }
    }
    #endregion
    public TabletPlayPodcastViewModel()
    {
    }
}
