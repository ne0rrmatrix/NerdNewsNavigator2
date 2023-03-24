// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="LivePage"/>
/// </summary>
public partial class LiveViewModel : BaseViewModel
{
    /// <summary>
    /// A <see cref="string"/> variable containing the URL for Live video for twit.tv
    /// </summary>
    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }
    /// <summary>
    /// A <see cref="string"/> variable containing the URL for Live video for twit.tv
    /// </summary>
    private string _url = "https://www.youtube.com/embed/F2NreNEmMy4";

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveViewModel"/>
    /// <paramref name="logger"/>
    /// </summary>
    public LiveViewModel(ILogger<LiveViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
    }
}
