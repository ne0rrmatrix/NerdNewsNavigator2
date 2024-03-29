﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;
/// <summary>
/// A class that manages displaying <see cref="Podcast"/> from twit.tv network.
/// </summary>
public partial class PodcastViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ILogger"/> class
    /// </summary>
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(PodcastViewModel));

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastViewModel"/> class.
    /// </summary>
    public PodcastViewModel(IConnectivity connectivity) : base(connectivity)
    {
        ThreadPool.QueueUserWorkItem(async (state) => await GetPodcasts());
        App.Downloads.DownloadStarted += DownloadStarted;

        // The following just sets the Title to string.empty when download is cancelled or completed.
        App.Downloads.DownloadFinished += DownloadCancelled;
        App.Downloads.DownloadCancelled += DownloadCancelled;
    }
    public ICommand PullToRefreshCommand => new Command(async () =>
    {
        _logger.Info("Refresh podcasts");
        IsRefreshing = true;
        IsBusy = true;
        await UpdatePodcasts();
        IsBusy = false;
        IsRefreshing = false;
        _logger.Info("Finished refreshing podcasts");
    });

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="ShowPage"/>
    /// </summary>
    /// <param name="podcast">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task GotoShowPage(Podcast podcast)
    {
        var encodedUrl = HttpUtility.UrlEncode(podcast.Url);
        await Shell.Current.GoToAsync($"{nameof(ShowPage)}?Url={encodedUrl}");
    }
}
