// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// a class that manages Updating settings.
/// </summary>
public partial class UpdateSettingsPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of <see cref="UpdateSettingsPage"/> class.
    /// </summary>
    /// <param name="viewModel">This pages <see cref="ViewModel"/> from <see cref="UpdateSettingsViewModel"/></param>
    public UpdateSettingsPage(UpdateSettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
