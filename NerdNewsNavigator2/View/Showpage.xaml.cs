// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that manages showing a <see cref="List{T}"/> of <see cref="Show"/> to users.
/// </summary>
public partial class ShowPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShowPage"/> class.
    /// </summary>
    /// <param name="viewModel">This pages <see cref="ViewModel"/> from <see cref="ShowViewModel"/></param>
    public ShowPage(ShowViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        DeviceService.RestoreScreen();
        if (App.IsDownloading)
        {
            Shell.SetNavBarIsVisible(Shell.Current.CurrentPage, true);
        }
    }
}
