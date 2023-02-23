// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that manages watching Live video from twit.tv podcasting network
/// </summary>
public partial class LivePage : ContentPage
{
    /// <summary>
    /// Initializes the <see cref="ILogger{TCategoryName}"/> instance in class <see cref="LivePage"/> 
    /// </summary>
    private readonly ILogger<LivePage> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="LivePage"/> class.
    /// </summary>
    /// <param name="liveViewModel">This classes <see cref="ViewModel"/> from <see cref="LiveViewModel"/></param>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance that is managed through this class.</param>
    public LivePage(LiveViewModel liveViewModel, ILogger<LivePage> logger)
    {
        InitializeComponent();
        BindingContext = liveViewModel;
        _logger = logger;
    }
}
