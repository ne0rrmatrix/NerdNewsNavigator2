﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NerdNewsNavigator2.IViews;

namespace NerdNewsNavigator2;
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(ViewService.ResolvePage<IPodcastPage>());
    }
}
