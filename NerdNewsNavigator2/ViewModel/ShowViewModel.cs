﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="ShowViewModel"/>
/// </summary>
public partial class ShowViewModel : SharedViewModel
{

    /// <summary>
    /// Initilizes a new instance of the <see cref="ILogger{TCategoryName}"/> class
    /// </summary>
    private readonly ILogger<SharedViewModel> _logger;
    /// <summary>
    /// Initializes a new instance of the <see cref="ShowViewModel"/> class.
    /// </summary>
    public ShowViewModel(ILogger<ShowViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        _logger = logger;
        App.Downloads.DownloadCancelled += UpdateOnCancel;
        App.CurrentNavigation.NavigationCompleted += OnNavigated;
        App.Downloads.DownloadFinished += ShowsDownloadCompleted;
        if (App.Downloads.Shows.Count > 0)
        {
            App.Downloads.DownloadStarted += DownloadStarted;
        }
    }
    public new ICommand PullToRefreshCommand => new Command(async () =>
    {
        _logger.LogInformation("Starting Show refresh");
        IsRefreshing = true;
        await RefreshData();
        IsRefreshing = false;
        _logger.LogInformation("Show Refresh is done");
    });
    public async Task RefreshData()
    {
        IsBusy = true;
        Shows.Clear();
        DownloadedShows.Clear();
        await GetDownloadedShows();
        GetShowsAsync(Url, false);
        IsBusy = false;
    }
    private async void ShowsDownloadCompleted(object sender, DownloadEventArgs e)
    {
        await GetDownloadedShows();
        Debug.WriteLine("Shows View model - Downloaded event firing");
        UpdateShows();
    }
}
