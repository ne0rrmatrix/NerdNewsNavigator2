// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="ShowViewModel"/>
/// </summary>
[QueryProperty("Url", "Url")]
public partial class ShowViewModel : BaseViewModel
{
    /// <summary>
    /// A private <see cref="string"/> that contains a Url for <see cref="Show"/>
    /// </summary>
    [ObservableProperty]
    private string _url;
    /// <summary>
    /// Initilizes a new instance of the <see cref="ILogger"/> class
    /// </summary>
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(ShowViewModel));
    /// <summary>
    /// Initializes a new instance of the <see cref="ShowViewModel"/> class.
    /// </summary>
    public ShowViewModel(IConnectivity connectivity) : base(connectivity)
    {
        App.Downloads.DownloadStarted += DownloadStarted;
        App.Downloads.DownloadFinished += ShowsDownloadCompleted;
        App.Downloads.DownloadCancelled += DownloadCancelled;
        App.DeletedItem.DeletedItem += OnItemDeleted;
    }
    partial void OnUrlChanged(string oldValue, string newValue)
    {
        _logger.Info("Show Url changed. Updating Shows");
        if (!InternetConnected())
        {
            return;
        }
        var decodedUrl = HttpUtility.UrlDecode(newValue);
        ThreadPool.QueueUserWorkItem(state => GetShowsAsync(decodedUrl, false));
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
        Title = e.Title;
        await GetDownloadedShows();
        Shows.Where(x => x.Title == e.Item.Title).ToList().ForEach(SetProperties);
    }
    [RelayCommand]
    public static void Cancel(Show show)
    {
        App.DownloadService.Cancel(show.Url);
        show.IsDownloading = false;
        show.IsNotDownloaded = true;
        show.IsDownloaded = false;
    }
    /// <summary>
    /// A Method that passes a Url to <see cref="DownloadService"/>
    /// </summary>
    /// <param name="show">A Url <see cref="Show"/></param>
    /// <returns></returns>
    [RelayCommand]
    public static void Download(Show show)
    {
#if ANDROID
        _ = EditViewModel.CheckAndRequestForeGroundPermission();
#endif
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Toast.Make("Added show to downloads.", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
        });
        show.IsDownloaded = false;
        show.IsDownloading = true;
        show.IsNotDownloaded = false;
        App.DownloadService.Add(show);
#if ANDROID || IOS
        _ = App.DownloadService.Start(show);
#else
        App.DownloadService.Start(show);
#endif
    }
}
