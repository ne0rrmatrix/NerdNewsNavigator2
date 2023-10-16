﻿// Licensed to the .NET Foundation under one or more agreements.
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
        App.DeletedItem.DeletedItem += OnItemDeleted;
        if (App.Downloads.Shows.Count > 0)
        {
            App.Downloads.DownloadStarted += DownloadStarted;
            App.Downloads.DownloadFinished += DownloadCompleted;
        }
    }
    partial void OnUrlChanged(string oldValue, string newValue)
    {
        _logger.Info("Show Url changed. Updating Shows");
        if (!InternetConnected())
        {
            return;
        }
        var decodedUrl = HttpUtility.UrlDecode(newValue);
        GetShowsAsync(decodedUrl, false);
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
    [RelayCommand]
    public void Cancel(string url)
    {
        Title = string.Empty;
        DownloadProgress = string.Empty;
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(DownloadProgress));
        App.Downloads.Cancel(url);
        var show = Shows.ToList().Find(x => x.Url == url);
        var number = Shows.IndexOf(show);
        show.IsDownloading = false;
        show.IsNotDownloaded = true;
        show.IsDownloaded = false;
        Shows[number] = show;
    }
    /// <summary>
    /// A Method that passes a Url to <see cref="DownloadService"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public void Download(string url)
    {
#if ANDROID
        _ = EditViewModel.CheckAndRequestForeGroundPermission();
#endif
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Toast.Make("Added show to downloads.", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
        });
        var item = Shows.ToList().Find(x => x.Url == url);
        var number = Shows.IndexOf(item);
        Shows[number].IsDownloaded = false;
        Shows[number].IsDownloading = true;
        Shows[number].IsNotDownloaded = false;
        if (App.Downloads.Shows.Count == 0)
        {
            _logger.Info($"Current download count is: {App.Downloads.Shows.Count}");
            App.Downloads.DownloadStarted += DownloadStarted;
            App.Downloads.DownloadCancelled += DownloadCancelled;
            App.Downloads.DownloadFinished += DownloadCompleted;
        }
        App.Downloads.Add(item);
#if ANDROID || IOS
        _ = App.Downloads.Start(item);
#else
        App.Downloads.Start(item);
#endif
    }
}
