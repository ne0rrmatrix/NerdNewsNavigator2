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
    /// Initializes a new instance of the <see cref="ShowViewModel"/> class.
    /// </summary>
    public ShowViewModel(ILogger<ShowViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        App.Downloads.DownloadCancelled += UpdateOnCancel;
        App.CurrentNavigation.NavigationCompleted += OnNavigated;
        App.Downloads.DownloadFinished += ShowsDownloadCompleted;
        if (App.Downloads.Shows.Count > 0)
        {
            App.Downloads.DownloadStarted += DownloadStarted;
        }
    }

    private async void ShowsDownloadCompleted(object sender, DownloadEventArgs e)
    {
        await GetDownloadedShows();
        Debug.WriteLine("Shows View model - Downloaded event firing");
        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            IsBusy = false;
            Title = string.Empty;
            DownloadProgress = string.Empty;
            Shows?.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(item =>
            {
                var number = Shows.IndexOf(item);
                Shows[number].IsDownloaded = true;
                Shows[number].IsDownloading = false;
                Shows[number].IsNotDownloaded = false;
                OnPropertyChanged(nameof(Shows));
            });
        });
    }
    [RelayCommand]
    public void Cancel(string url)
    {
        Title = string.Empty;
        SetCancelData(url, true);
    }
}
