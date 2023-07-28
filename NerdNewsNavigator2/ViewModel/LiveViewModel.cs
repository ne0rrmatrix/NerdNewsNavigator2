// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="LivePage"/>
/// </summary>
public partial class LiveViewModel : SharedViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LiveViewModel"/>
    /// <paramref name="connectivity"/>
    /// </summary>
    public LiveViewModel(IConnectivity connectivity) : base(connectivity)
    {
    }
}
