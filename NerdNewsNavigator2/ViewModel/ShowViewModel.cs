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
        Shows = new ObservableCollection<Show>();
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
        Debug.WriteLine("Shows View Model - Downloaded event firing");
        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            IsBusy = false;
            Title = string.Empty;
            DownloadProgress = string.Empty;
            var show = Shows.ToList().Exists(x => x.Url == e.Item.Url);
            if (show)
            {
                var item = Shows.ToList().Find(x => x.Url == e.Item.Url);
                var number = Shows.IndexOf(item);
                Shows[number].IsDownloaded = true;
                Shows[number].IsDownloading = false;
                Shows[number].IsNotDownloaded = false;
                OnPropertyChanged(nameof(Shows));
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
        SetCancelData(item, true);
    }
}
