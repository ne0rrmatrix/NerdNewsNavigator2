// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="MostRecentShowsViewModel"/>
/// </summary>
public partial class MostRecentShowsViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of <see cref="MostRecentShowsViewModel"/>
    /// <paramref name="logger"/>
    /// </summary>
    public MostRecentShowsViewModel(ILogger<MostRecentShowsViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        WeakReferenceMessenger.Default.Register<DownloadStatusMessage>(this, (r, m) =>
        {
            RecievedDownloadSttusMessage(m.Value, m.Now);
        });
        OnPropertyChanged(nameof(IsBusy));
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        Orientation = OnDeviceOrientationChange();
        if (!InternetConnected())
        {
            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
        }
#if WINDOWS || MACCATALYST || IOS
        if (DownloadService.IsDownloading)
        {
            ThreadPool.QueueUserWorkItem(state => { UpdatingDownload(); });
        }
#endif
#if WINDOWS || ANDROID
        Task.Run(GetMostRecent);
#endif
#if IOS || MACCATALYST
        _ = GetMostRecent();
#endif
    }
    public void RecievedDownloadSttusMessage(bool value, Show item)
    {
        if (item is not null)
        {
            item.IsDownloading = value;
            _ = MainThread.InvokeOnMainThreadAsync(() => Dnow.Update(item));
        }
    }

    #region Events

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="MostRecentShowsPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Tap(string url) => await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}?Url={url}");

    #endregion
}
