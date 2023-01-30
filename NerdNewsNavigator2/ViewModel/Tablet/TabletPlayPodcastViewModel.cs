﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel.Tablet;

[QueryProperty("Url", "Url")]
public partial class TabletPlayPodcastViewModel : ObservableObject
{
    #region Properties
    [ObservableProperty]
    public string _url;
    #endregion
}