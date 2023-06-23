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
        if (DownloadService.IsDownloading)
        {
            ThreadPool.QueueUserWorkItem(state => { UpdatingDownload(); });
        }
    }
    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="ShowPage"/>
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public Task UpdatePodcasts()
    {
        ThreadPool.QueueUserWorkItem(async (state) =>
       {
           var res = await PodcastServices.UpdatePodcast();
           Podcasts.Clear();
           res.ForEach(Podcasts.Add);

           var fav = await PodcastServices.UpdateFavorites();
           FavoriteShows.Clear();
           fav.ForEach(FavoriteShows.Add);

           await MainThread.InvokeOnMainThreadAsync(async () =>
           {
               await Shell.Current.GoToAsync($"{nameof(PodcastPage)}");
           });
       });
        return Task.CompletedTask;
    }
}
