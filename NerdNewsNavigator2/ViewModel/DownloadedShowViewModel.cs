﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="DownloadedShowViewModel"/>
/// </summary>
public partial class DownloadedShowViewModel : BaseViewModel
{

    /// <summary>
    /// Initilizes a new instance of the <see cref="ILogger"/> class
    /// </summary>
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(DownloadedShowViewModel));

    /// <summary>
    /// Intilializes an instance of <see cref="DownloadedShowViewModel"/>
    /// <paramref name="connectivity"/>
    /// </summary>
    public DownloadedShowViewModel(IConnectivity connectivity)
        : base(connectivity)
    {
        App.Downloads.DownloadStarted += DownloadStarted;
        App.Downloads.DownloadCancelled += DownloadCancelled;
        App.Downloads.DownloadFinished += ShowsDownloadCompleted;
    }
    public ICommand PullToRefreshCommand => new Command(async () =>
    {
        _logger.Info("Starting refresh of Downloaded shows");
        IsRefreshing = true;
        await RefreshData();
        IsRefreshing = false;
        _logger.Info("Finished refreshing of Downloaded shows");
    });
    public async Task RefreshData()
    {
        IsBusy = true;
        DownloadedShows.Clear();
        await GetDownloadedShows();
        IsBusy = false;
    }
    private void ShowsDownloadCompleted(object sender, DownloadEventArgs e)
    {
        _ = MainThread.InvokeOnMainThreadAsync(async () =>
        {
            Title = string.Empty;
            OnPropertyChanged(Title);
            await GetDownloadedShows();
        });
    }

    /// <summary>
    /// Method Deletes a <see cref="Download"/>
    /// </summary>
    /// <param name="download"><see cref="Download"/> to be Deleted.</param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Delete(Download download)
    {
        var tempFile = FileService.GetFileName(download.Url);
        FileService.DeleteFile(tempFile);
        download.IsDownloaded = false;
        download.Deleted = true;
        download.IsNotDownloaded = true;
        await App.PositionData.UpdateDownload(download);
        DownloadedShows.Remove(download);
        var showTemp = Shows.ToList().Find(x => x.Url == download.Url);
        Shows?.Remove(showTemp);
        _logger.Info($"Removed {download.FileName} from Downloaded Shows list.");
        App.DeletedItem.Add(download);
    }
}

