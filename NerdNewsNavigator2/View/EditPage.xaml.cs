// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that manages removing a <see cref="Podcast"/> from <see cref="List{T}"/> of <see cref="Podcast"/>
/// </summary>
public partial class EditPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of <see cref="EditPage"/>
    /// </summary>
    /// <param name="viewModel">This Pages <see cref="ViewModel"/> from <see cref="EditViewModel"/></param>
    public EditPage(EditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        CustomControls.RestoreScreen();
    }
}
