// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="VideoPlayerViewModel"/>
/// </summary>
[QueryProperty("Url", "Url")]
public partial class VideoPlayerViewModel : BaseViewModel
{
    /// <summary>
    /// A private <see cref="string"/> that contains a Url for <see cref="Show"/>
    /// </summary>
    #region Properties
    private string _url;

    /// <summary>
    /// A public <see cref="string"/> that contains a Url for <see cref="Show"/>
    /// </summary>
    public string Url
    {
        get => _url;
        set
        {
            SetProperty(ref _url, value);
            VideoPlayerPage.Url = value;
        }
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoPlayerViewModel"/> class.
    /// </summary>
    public VideoPlayerViewModel(ILogger<VideoPlayerViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
    }
}
