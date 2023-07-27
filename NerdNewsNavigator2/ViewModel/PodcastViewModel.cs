// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;
/// <summary>
/// A class that manages displaying <see cref="Podcast"/> from twit.tv network.
/// </summary>
public partial class PodcastViewModel : SharedViewModel
{
    /// <summary>
    /// Initilizes a new instance of the <see cref="ILogger{TCategoryName}"/> class
    /// </summary>
    private readonly ILogger<PodcastViewModel> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastViewModel"/> class.
    /// </summary>
    public PodcastViewModel(ILogger<PodcastViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        _logger = logger;
        ThreadPool.QueueUserWorkItem(async (state) => await GetUpdatedPodcasts());
        if (App.Downloads.Shows.Count > 0)
        {
            App.Downloads.DownloadStarted += DownloadStarted;
        }
    }
    public ICommand PullToRefreshCommand => new Command(async () =>
    {
        _logger.LogInformation("Refresh podcasts");
        IsRefreshing = true;
        await RefreshData();
        IsRefreshing = false;
        _logger.LogInformation("Finished refreshing podcasts");
    });
    public async Task RefreshData()
    {
        IsBusy = true;
        Podcasts.Clear();
        await GetUpdatedPodcasts();
        IsBusy = false;
    }
    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="ShowPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task GotoShowPage(string url)
    {
        var encodedUrl = HttpUtility.UrlEncode(url);
        await Shell.Current.GoToAsync($"{nameof(ShowPage)}?Url={encodedUrl}");
    }
}
