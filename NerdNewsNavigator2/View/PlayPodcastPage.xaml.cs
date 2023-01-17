﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NerdNewsNavigator2.ViewModel;

namespace NerdNewsNavigator2.View;

public partial class PlayPodcastPage : ContentPage
{
    public PlayPodcastPage(PlayPodcastViewModel viewmodel)
    {
        InitializeComponent();
        BindingContext = viewmodel;
    }
}
