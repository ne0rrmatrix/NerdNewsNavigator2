// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Logging;

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
    }

    [RelayCommand]
    public void Cancel(string url)
    {
        var item = Shows.ToList().Find(x => x.Url == url);
        App.CurrenDownloads?.Remove(item);
        item.IsDownloading = false;
        item.IsNotDownloaded = true;
        item.IsDownloaded = false;
        Shows[Shows.IndexOf(item)] = item;
        IsDownloading = false;
        if (App.CurrenDownloads.Count == 0)
        {
            DownloadService.IsDownloading = false;
            DownloadService.CancelDownload = true;
        }
    }
    /// <summary>
    /// A Method that passes a Url to <see cref="DownloadService"/>
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public void Download(string url)
    {
        /*
        if (DownloadService.IsDownloading)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Toast.Make("Please wait for the download to finish...", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
            });
            return;
        }
        */
#if ANDROID
        _ = EditViewModel.CheckAndRequestForeGroundPermission();
#endif
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Toast.Make("Added show to downloads.", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
        });

        _ = Task.Run(async () =>
        {
            await Downloading(url);
            var item = GetShowForDownload(url);
            if (item is not null && App.CurrenDownloads.Exists(x => x.Url == item.Url))
            {
                App.CurrenDownloads?.Remove(item);
                item.IsDownloaded = true;
                item.IsDownloading = false;
                item.IsNotDownloaded = false;
                if (Shows.ToList().Exists(x => x.Url == item.Url))
                {
                    Shows[Shows.IndexOf(item)] = item;
                }
            }
        });
    }

}
