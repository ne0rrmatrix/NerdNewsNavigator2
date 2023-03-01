// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A Class that extends <see cref="BaseViewModel"/> for <see cref="AddPodcastPage"/>
/// </summary>
public partial class AddPodcastViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddPodcastViewModel"/>
    /// </summary>
    public AddPodcastViewModel(ILogger<AddPodcastViewModel> logger)
        : base(logger)
    {
    }
}
