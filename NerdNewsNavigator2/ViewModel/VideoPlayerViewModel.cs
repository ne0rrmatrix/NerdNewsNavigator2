﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="VideoPlayerViewModel"/>
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="VideoPlayerViewModel"/> class.
/// </remarks>
public partial class VideoPlayerViewModel(IConnectivity connectivity) : BaseViewModel(connectivity)
{
}
