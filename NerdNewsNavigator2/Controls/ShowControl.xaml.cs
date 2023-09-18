// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Maui.BindableProperty.Generator.Core;

namespace NerdNewsNavigator2.Controls;

public partial class ShowControl : ContentView
{
#pragma warning disable IDE0051, IDE0052, CS0169
    [AutoBindable(DefaultBindingMode = nameof(BindingMode.TwoWay))]
    private readonly ObservableCollection<Show> _source;
#pragma warning restore IDE0051, IDE0052, CS0169
    public ShowControl()
    {
        InitializeComponent();
    }
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        CustomControls.RestoreScreen();
    }
}
