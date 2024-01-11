// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Maui.Behaviors;

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that displays a <see cref="List{T}"/> of <see cref="Podcast"/> from twit.tv network.
/// </summary>
public partial class PodcastPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastPage"/> class.
    /// </summary>
    /// <param name="viewModel">This pages <see cref="ViewModel"/> from <see cref="PodcastViewModel"/></param>
    public PodcastPage(PodcastViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

#if ANDROID || IOS16_1_OR_GREATER
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
#pragma warning disable CA1416 // Validate platform compatibility
        this.Behaviors.Add(new StatusBarBehavior
        {
            StatusBarColor = Color.FromArgb("#000000")
        });
#pragma warning restore CA1416 // Validate platform compatibility
    }
#endif
}
