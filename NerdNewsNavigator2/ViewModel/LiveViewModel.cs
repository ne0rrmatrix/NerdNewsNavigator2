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
    /// Initializes a new instance of the <see cref="LiveViewModel"/>
    /// <paramref name="logger"/>
    /// </summary>
    public LiveViewModel(ILogger<LiveViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
#if IOS
        IsBusy = false;
#endif
    }

    /// <summary>
    /// A Method that detects mouse movment on <see cref="LivePage"/>
    /// </summary>
    [RelayCommand]
    public async Task Moved()
    {
        if (!IsBusy)
        {
            await SetIsBusy();
        }
    }
}
