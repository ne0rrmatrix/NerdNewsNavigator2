// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2;

/// <summary>
/// A class that acts as a manager for <see cref="Application"/>
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// This applications Dependancy Injection for <see cref="PositionDataBase"/> class.
    /// </summary>
    public static PositionDataBase PositionData { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="positionDataBase"></param>
    public App(PositionDataBase positionDataBase)
    {
        InitializeComponent();
        MainPage = new AppShell();

        // Database Dependancy Injection START
        PositionData = positionDataBase;
        // Database Dependancy Injection END

        LogController.InitializeNavigation(
            page => MainPage!.Navigation.PushModalAsync(page),
            () => MainPage!.Navigation.PopModalAsync());
    }
}

