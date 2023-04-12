// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits form <see cref="BaseViewModel"/> and manages showing <see cref="Show"/> information.
/// </summary>
[QueryProperty("Url", "Url")]
public partial class ShowViewModel : BaseViewModel
{
    #region Properties

    private string _url;
    /// <summary>
    /// A Url <see cref="string"/> containing the <see cref="Show"/>
    /// </summary>
    public string Url
    {
        get { return _url; }
        set
        {
            _url = value;
            var decodedUrl = HttpUtility.UrlDecode(value);
            _ = GetShows(decodedUrl, false);
            OnPropertyChanged(nameof(Shows));
        }
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ShowViewModel"/> class.
    /// </summary>
    public ShowViewModel(ILogger<ShowViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        Orientation = OnDeviceOrientationChange();
        if (!InternetConnected())
        {
            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
        }
        if (App.IsDownloading)
        {
            ThreadPool.QueueUserWorkItem(state => { UpdatingDownload(); });
        }
    }

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="PodcastPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Tap(string url)
    {
        await Shell.Current.GoToAsync($"{nameof(VideoPlayerPage)}?Url={url}");
    }

    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="ShowPage"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Download(string url)
    {
        await Downloading(url, false);
    }

}
