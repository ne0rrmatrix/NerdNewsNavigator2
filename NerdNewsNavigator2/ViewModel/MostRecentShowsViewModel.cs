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
    /// Initializes a new instance of <see cref="MostRecentShowsViewModel"/>
    /// <paramref name="logger"/>
    /// </summary>
    public MostRecentShowsViewModel(ILogger<MostRecentShowsViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
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

    private async void MostRecentDownloadCompleted(object sender, DownloadEventArgs e)
    {
        await GetDownloadedShows();
        Debug.WriteLine("MostRecent View model - Downloaded event firing");
        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            IsBusy = false;
            Title = string.Empty;
            DownloadProgress = string.Empty;
            MostRecentShows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(item =>
            {
                var number = MostRecentShows.IndexOf(item);
                MostRecentShows[number].IsDownloaded = true;
                MostRecentShows[number].IsDownloading = false;
                MostRecentShows[number].IsNotDownloaded = false;
                OnPropertyChanged(nameof(MostRecentShows));
            });
        });
    }

    [RelayCommand]
    public void Cancel(string url)
    {
        Title = string.Empty;
        SetCancelData(url, false);
    }
}
