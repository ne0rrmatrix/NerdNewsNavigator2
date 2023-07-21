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
        if (MostRecentShows.ToList().Count == 0)
        {
            ThreadPool.QueueUserWorkItem(async state => await GetMostRecent());
        }
        if (App.Downloads.Shows.Count > 0)
        {
            App.Downloads.DownloadFinished += DownloadCompleted;
        }
    }
    private void DownloadCompleted(object sender, DownloadEventArgs e)
    {
        if (e.Status == string.Empty)
        {
            Title = string.Empty;
        }
        if (App.Downloads.Shows.Count == 0)
        {
            Title = string.Empty;
            App.Downloads.DownloadFinished -= DownloadCompleted;
        }
        Debug.WriteLine("Most Recent Shows Viewmodel - Downloaded event firing");
        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            IsBusy = false;
            Title = string.Empty;
            DownloadProgress = string.Empty;
            var show = MostRecentShows.ToList().Exists(x => x.Url == e.Item.Url);
            if (show)
            {
                var item = MostRecentShows.ToList().Find(x => x.Url == e.Item.Url);
                var number = MostRecentShows.IndexOf(item);
                MostRecentShows[number].IsDownloaded = true;
                MostRecentShows[number].IsDownloading = false;
                MostRecentShows[number].IsNotDownloaded = false;
                OnPropertyChanged(nameof(MostRecentShows));
            }
        });
    }

    [RelayCommand]
    public void Cancel(string url)
    {
        var item = App.Downloads.Cancel(url);
        if (item != null)
        {
            Debug.WriteLine(item.Url);
        }
        Title = string.Empty;
        SetCancelData(item, false);
    }
}
