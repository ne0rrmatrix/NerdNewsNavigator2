// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that manages application settings.
/// </summary>
public partial class SettingsPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPage"/> instance.
    /// </summary>
    /// <param name="viewModel">This Pages <see cref="ViewModel"/> from <see cref="SettingsViewModel"/></param>
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        DeviceService.RestoreScreen();
    }
}
