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
    /// Initilizes a new instance of the <see cref="ILogger"/> class
    /// </summary>
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(ShowViewModel));
    /// <summary>
    /// Initializes a new instance of the <see cref="ShowViewModel"/> class.
    /// </summary>
    public ShowViewModel(IConnectivity connectivity) : base(connectivity)
    {
        Shows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        Shows?.Where(x => App.Downloads.Shows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(SetProperties);
        App.DeletedItem.DeletedItem += OnItemDeleted;
        if (App.Downloads.Shows.Count > 0)
        {
            App.Downloads.DownloadStarted += DownloadStarted;
            App.Downloads.DownloadFinished += DownloadCompleted;
        }
    }
    private async void OnItemDeleted(object sender, DeletedItemEventArgs e)
    {
        await GetDownloadedShows();
        _logger.Info("Updating deleted Items");
        Shows?.Where(x => x.Url == e.Item.Url).ToList().ForEach(SetProperties);
    }
    public ICommand PullToRefreshCommand => new Command(async () =>
    {
        _logger.Info("Starting Show refresh");
        IsRefreshing = true;
        await RefreshData();
        IsRefreshing = false;
        _logger.Info("Show Refresh is done");
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
        UpdateShows();
    }
}
