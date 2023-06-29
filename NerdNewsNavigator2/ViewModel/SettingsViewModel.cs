// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A Class that extends <see cref="BaseViewModel"/> for <see cref="SettingsViewModel"/>
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/>
    /// </summary>
    public SettingsViewModel(ILogger<SettingsViewModel> logger, IConnectivity connectivity)
        : base(logger, connectivity)
    {
#if WINDOWS || MACCATALYST || IOS
        if (DownloadService.IsDownloading)
        {
            ThreadPool.QueueUserWorkItem(state => { UpdatingDownload(); });
        }
#endif
    }
    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="ShowPage"/>
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task UpdatePodcasts()
    {
        await Toast.Make("Updating Podcasts.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        ThreadPool.QueueUserWorkItem(async (state) =>
       {
           _ = await PodcastServices.UpdatePodcast();
           Podcasts.Clear();

           _ = await PodcastServices.UpdateFavoritesAsync();
           FavoriteShows.Clear();

           await MainThread.InvokeOnMainThreadAsync(async () =>
           {
               await Shell.Current.GoToAsync($"{nameof(PodcastPage)}");
           });
       });
    }
}
