// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// a class that manages Updating settings.
/// </summary>
public partial class ResetAllSettingsPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of <see cref="ResetAllSettingsPage"/> class.
    /// </summary>
    /// <param name="viewModel">This pages <see cref="ViewModel"/> from <see cref="ResetAllSettingsViewModel"/></param>
    public ResetAllSettingsPage(ResetAllSettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
