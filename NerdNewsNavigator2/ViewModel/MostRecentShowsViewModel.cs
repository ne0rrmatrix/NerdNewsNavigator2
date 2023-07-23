// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Messaging;

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="MostRecentShowsViewModel"/>
/// </summary>
public partial class MostRecentShowsViewModel : SharedViewModel, IRecipient<PageMessage>
{
    /// <summary>
    /// Initializes a new instance of <see cref="MostRecentShowsViewModel"/>
    /// <paramref name="logger"/>
    /// </summary>
    public MostRecentShowsViewModel(ILogger<MostRecentShowsViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        WeakReferenceMessenger.Default.Register<PageMessage>(this);
        if (MostRecentShows.ToList().Count == 0)
        {
            ThreadPool.QueueUserWorkItem(async state => await GetMostRecent());
        }
        if (App.Downloads.Shows.Count > 0)
        {
            App.Downloads.DownloadFinished += DownloadCompleted;
        }
    }

    [RelayCommand]
    public void Cancel(string url)
    {
        SetCancelData(url, false);
    }

    public async void Receive(PageMessage message)
    {
        WeakReferenceMessenger.Default.Unregister<PageMessage>(message);
        Debug.WriteLine("Received message on MostRecentViewModel");
        await GetDownloadedShows();
        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            MostRecentShows.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(item =>
            {
                var number = MostRecentShows.IndexOf(item);
                MostRecentShows[number].IsDownloaded = false;
                MostRecentShows[number].IsDownloading = false;
                MostRecentShows[number].IsNotDownloaded = true;
                OnPropertyChanged(nameof(MostRecentShows));
            });
            MostRecentShows.Where(x => App.Downloads.Shows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(item =>
            {
                var number = MostRecentShows.IndexOf(item);
                MostRecentShows[number].IsDownloaded = false;
                MostRecentShows[number].IsDownloading = true;
                MostRecentShows[number].IsNotDownloaded = false;
                OnPropertyChanged(nameof(MostRecentShows));
            });
            if (App.Downloads.Shows.Count == 0)
            {
                MostRecentShows.Where(x => !DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(item =>
                {
                    var number = MostRecentShows.IndexOf(item);
                    MostRecentShows[number].IsDownloaded = false;
                    MostRecentShows[number].IsDownloading = false;
                    MostRecentShows[number].IsNotDownloaded = true;
                    OnPropertyChanged(nameof(MostRecentShows));
                });
                Title = string.Empty;
            }
        });
    }
}
