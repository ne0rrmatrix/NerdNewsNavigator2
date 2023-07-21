// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="ShowViewModel"/>
/// </summary>

public partial class ShowViewModel : SharedViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShowViewModel"/> class.
    /// </summary>
    public ShowViewModel(ILogger<ShowViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        Shows = new ObservableCollection<Show>();
        if (App.Downloads.Shows.Count > 0)
        {
            App.Downloads.DownloadFinished += DownloadCompleted;
        }
    }

    [RelayCommand]
    public void Cancel(string url)
    {
        SetCancelData(url, true);
    }
}
