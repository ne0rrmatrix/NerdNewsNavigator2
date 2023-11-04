// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A Class that extends <see cref="BaseViewModel"/> for <see cref="SettingsViewModel"/>
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SettingsViewModel"/>
/// </remarks>
public partial class SettingsViewModel : BaseViewModel
{
    public SettingsViewModel(IConnectivity connectivity) : base(connectivity)
    {
    }
}
