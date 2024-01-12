// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if ANDROID || IOS16_1_OR_GREATER
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core.Platform;
#endif

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that Displays a Video from twit.tv.
/// </summary>
public partial class VideoPlayerPage : ContentPage
{
    /// <summary>
    /// Class Constructor that initializes <see cref="VideoPlayerPage"/>
    /// </summary>
    /// <param name="viewModel">This Applications <see cref="VideoPlayerPage"/> instance is managed through this class.</param>

    public VideoPlayerPage(VideoPlayerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

#if ANDROID || IOS16_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
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

#if ANDROID || IOS16_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        this.Behaviors.Add(new StatusBarBehavior
        {
            StatusBarColor = Color.FromArgb("#34AAD2")
        });
#pragma warning restore CA1416 // Validate platform compatibility
        base.OnNavigatedFrom(args);
    }
#endif
}
