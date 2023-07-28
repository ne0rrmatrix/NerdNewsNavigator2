﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="MostRecentShowsViewModel"/>
/// </summary>
public partial class MostRecentShowsViewModel : SharedViewModel
{
    /// <summary>
    /// Initilizes a new instance of the <see cref="ILogger"/> class
    /// </summary>
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(MostRecentShowsViewModel));

    /// <summary>
    /// Initializes a new instance of <see cref="MostRecentShowsViewModel"/>
    /// <paramref name="connectivity"/>
    /// </summary>
    public MostRecentShowsViewModel(IConnectivity connectivity) : base(connectivity)
    {
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
        _logger.Info("Refresh Most recent shows");
        IsRefreshing = true;
        await RefreshData();
        IsRefreshing = false;
        _logger.Info("Finished refreshing Most recent shows");
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
