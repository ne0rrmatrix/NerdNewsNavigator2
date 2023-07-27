// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="MostRecentShowsViewModel"/>
/// </summary>
public partial class MostRecentShowsViewModel : SharedViewModel
{
    /// <summary>
    /// Initilizes a new instance of the <see cref="ILogger{TCategoryName}"/> class
    /// </summary>
    private readonly ILogger<MostRecentShowsViewModel> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="MostRecentShowsViewModel"/>
    /// <paramref name="logger"/>
    /// </summary>
    public MostRecentShowsViewModel(ILogger<MostRecentShowsViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        _logger = logger;
        App.Downloads.DownloadCancelled += UpdateOnCancel;
        App.CurrentNavigation.NavigationCompleted += OnNavigated;
        App.Downloads.DownloadFinished += MostRecentDownloadCompleted;
        if (MostRecentShows.ToList().Count == 0)
        {
            ThreadPool.QueueUserWorkItem(async state => await GetMostRecent());
        }
        if (App.Downloads.Shows.Count > 0)
        {
            App.Downloads.DownloadStarted += DownloadStarted;
        }
    }
    public ICommand PullToRefreshCommand => new Command(async () =>
    {
        _logger.LogInformation("Refresh Most recent shows");
        IsRefreshing = true;
        await RefreshData();
        IsRefreshing = false;
        _logger.LogInformation("Finished refreshing Most recent shows");
    });
    public async Task RefreshData()
    {
        IsBusy = true;
        App.MostRecentShows.Clear();
        MostRecentShows.Clear();
        DownloadedShows.Clear();
        await GetDownloadedShows();
        await GetMostRecent();
        IsBusy = false;
    }
    private async void MostRecentDownloadCompleted(object sender, DownloadEventArgs e)
    {
        await GetDownloadedShows();
        UpdateShows();
    }
}
