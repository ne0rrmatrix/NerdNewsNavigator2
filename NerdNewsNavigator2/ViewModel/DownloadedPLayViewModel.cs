// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that extends <see cref="BaseViewModel"/> and manages <see cref="DownloadPlayPage"/>
/// </summary>

[QueryProperty("Url", "Url")]
public partial class DownloadedPlayViewModel : BaseViewModel
{
    #region Properties

    /// <summary>
    /// A private <see cref="string"/> that contains a Url for <see cref="Show"/>
    /// </summary>
    private string _url;

    /// <summary>
    /// A public facing <see cref="string"/> that contains a Url for <see cref="Show"/>
    /// </summary>
    public string Url
    {
        get => _url;
        set
        {
            SetProperty(ref _url, value);
            Preferences.Default.Remove("New_Url", null);
            Preferences.Default.Set("New_Url", value);
            Debug.WriteLine($"Url is: {value}");
        }
    }
    #endregion

    /// <summary>
    /// Initializes the instance of <see cref="DownloadedPlayViewModel"/>
    /// </summary>
    /// <param name="logger"></param>
    public DownloadedPlayViewModel(ILogger<DownloadedPlayViewModel> logger)
    : base(logger)
    {
    }
}
