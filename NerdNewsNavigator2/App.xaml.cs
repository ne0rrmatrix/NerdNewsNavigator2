// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NerdNewsNavigator2.Data;
using Application = Microsoft.Maui.Controls.Application;
using Platform = Microsoft.Maui.ApplicationModel.Platform;

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT;
using Microsoft.Maui.Controls;
#endif

namespace NerdNewsNavigator2;
public partial class App : Application
{
    // DataBase Dependancy Injection START
    public static PositionDataBase PositionData { get; private set; }
    // DataBase Dependancy Injection END
    public App(PositionDataBase positionDataBase)
    {
        InitializeComponent();
        MainPage = new AppShell();

        // Database Dependancy Injection START
        PositionData = positionDataBase;
        // Database Dependancy Injection END
    }
}
