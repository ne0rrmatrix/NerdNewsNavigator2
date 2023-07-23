// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="ShowViewModel"/>
/// </summary>

public partial class ShowViewModel : SharedViewModel, IRecipient<PageMessage>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShowViewModel"/> class.
    /// </summary>
    public ShowViewModel(ILogger<ShowViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        WeakReferenceMessenger.Default.Register<PageMessage>(this);
        if (App.Downloads.Shows.Count > 0)
        {
            App.Downloads.DownloadFinished += DownloadCompleted;
        }
    }
    public static Command VBackCommand
    {
        get
        {
            return new Command(() =>
            {
                // if parameter are set, you could send a message to navigate
                WeakReferenceMessenger.Default.Send(new PageMessage(App.Downloads.Status, true));
                Shell.Current.GoToAsync("..");
            });
        }
    }
    [RelayCommand]
    public void Cancel(string url)
    {
        SetCancelData(url, true);
    }

    public async void Receive(PageMessage message)
    {
        WeakReferenceMessenger.Default.Unregister<PageMessage>(message);
        Debug.WriteLine("Received message on ShowViewModel");
        if (message.Value == "true")
        {
            Title = string.Empty;
        }
        await GetDownloadedShows();
        _ = MainThread.InvokeOnMainThreadAsync(() =>
        {
            Shows.Where(x => DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(item =>
            {
                var number = Shows.IndexOf(item);
                Shows[number].IsDownloaded = false;
                Shows[number].IsDownloading = false;
                Shows[number].IsNotDownloaded = true;
                OnPropertyChanged(nameof(Shows));
            });
            Shows.Where(x => App.Downloads.Shows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(item =>
            {
                var number = Shows.IndexOf(item);
                Shows[number].IsDownloaded = false;
                Shows[number].IsDownloading = true;
                Shows[number].IsNotDownloaded = false;
                OnPropertyChanged(nameof(Shows));
            });
            if (App.Downloads.Shows.Count == 0)
            {
                Shows.Where(x => !DownloadedShows.ToList().Exists(y => y.Url == x.Url)).ToList().ForEach(item =>
                {
                    var number = Shows.IndexOf(item);
                    Shows[number].IsDownloaded = false;
                    Shows[number].IsDownloading = false;
                    Shows[number].IsNotDownloaded = true;
                    OnPropertyChanged(nameof(Shows));
                });

                Title = string.Empty;
            }
        });
    }
}
