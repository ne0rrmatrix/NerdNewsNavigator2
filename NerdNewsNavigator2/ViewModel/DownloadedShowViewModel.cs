// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="SharedViewModel"/> and manages <see cref="DownloadedShowViewModel"/>
/// </summary>
public partial class DownloadedShowViewModel : SharedViewModel
{

    /// <summary>
    /// Initilizes a new instance of the <see cref="ILogger{TCategoryName}"/> class
    /// </summary>
    private readonly ILogger<DownloadedShowViewModel> _logger;

    /// <summary>
    /// Intilializes an instance of <see cref="DownloadedShowViewModel"/>
    /// <paramref name="logger"/>
    /// </summary>
    public DownloadedShowViewModel(ILogger<DownloadedShowViewModel> logger, IConnectivity connectivity)
        : base(logger, connectivity)
    {
        _logger = logger;
        if (App.Downloads.Shows.Count > 0)
        {
            App.Downloads.DownloadStarted += DownloadStarted;
        }
    }
    public new ICommand PullToRefreshCommand => new Command(async () =>
    {
        _logger.LogInformation("Starting refresh of Downloaded shows");
        IsRefreshing = true;
        await RefreshData();
        IsRefreshing = false;
        _logger.LogInformation("Finished refreshing of Downloaded shows");
    });
    public async Task RefreshData()
    {
        IsBusy = true;
        DownloadedShows.Clear();
        await GetDownloadedShows();
        IsBusy = false;
    }
}

