// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Logging;

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
        ThreadPool.QueueUserWorkItem(async (state) => await GetMostRecent());
    }
    [RelayCommand]
    public void Cancel(string url)
    {
        var item = MostRecentShows.ToList().Find(x => x.Url == url);
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
                if (MostRecentShows.ToList().Exists(x => x.Url == item.Url))
                {
                    MostRecentShows[MostRecentShows.IndexOf(item)] = item;
                }
            }
        });
    }

}
