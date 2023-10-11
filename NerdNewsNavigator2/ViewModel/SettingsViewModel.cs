// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A Class that extends <see cref="BaseViewModel"/> for <see cref="SettingsViewModel"/>
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SettingsViewModel"/>
/// </remarks>
public partial class SettingsViewModel(IConnectivity connectivity) : SharedViewModel(connectivity)
{
    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="ShowPage"/>
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task UpdatePodcasts()
    {
        await Toast.Make("Updating Podcasts.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        IsBusy = true;
        ThreadPool.QueueUserWorkItem(async (state) =>
       {
           var podcast = await PodcastServices.UpdatePodcast();
           Podcasts.Clear();
           podcast.ForEach(Podcasts.Add);
           var fav = await PodcastServices.UpdateFavoritesAsync();
           FavoriteShows.Clear();
           fav.ForEach(FavoriteShows.Add);
           _ = Task.Run(GetUpdatedPodcasts);
           IsBusy = false;
       });
    }
}
