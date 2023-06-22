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
            await UpdatePodcast();
            await UpdateFavorites();
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.GoToAsync($"{nameof(PodcastPage)}");
            });
        });
        return Task.CompletedTask;
    }

    private async Task UpdatePodcast()
    {
        var podcasts = await App.PositionData.GetAllPodcasts();
        await App.PositionData.DeleteAllPodcasts();

        // list of stale podcasts
        var stalePodcasts = podcasts.Where(x => x.Deleted).ToList();

        var newPodcasts = await RemoveStalePodcastsAsync(stalePodcasts);

        await AddPodcastsToDBAsync(stalePodcasts, newPodcasts);
    }
    private static async Task<List<Podcast>> RemoveStalePodcastsAsync(List<Podcast> stalePodcasts)
    {
        // get updated podcast list
        var newPodcasts = await PodcastServices.GetFromUrl();

        // remove stale podcasts
        if (stalePodcasts.Count > 0)
        {
            newPodcasts.ForEach(x =>
            {
                if (!stalePodcasts.Exists(y => y.Deleted == x.Deleted))
                {
                    newPodcasts.Remove(x);
                }
            });
        }
        return newPodcasts;
    }
    private async Task AddPodcastsToDBAsync(List<Podcast> stalePodcasts, List<Podcast> newPodcasts)
    {
        Podcasts.Clear();
        var res = new List<Podcast>();

        // add all podcasts that are not stale, add all new podcasts if any
        if (stalePodcasts.Count > 0)
        {
            Debug.WriteLine("Found stale podcasts");
            newPodcasts?.ForEach(x =>
            {
                if (!stalePodcasts.Any(y => y.Title == x.Title))
                {
                    res.Add(x);
                }
            });
        }
        else
        {
            Debug.WriteLine("Did not find any stale Podcasts");
            newPodcasts.ForEach(res.Add);
        }
        // sort podcast alphabetically
        res = res.OrderBy(x => x.Title).ToList();
        res.ForEach(Podcasts.Add);

        await PodcastServices.AddToDatabase(res);
    }
    private async Task UpdateFavorites()
    {
        // get old favorites list
        var favoriteShows = await App.PositionData.GetAllFavorites();

        // if favorite podcasts are stale remove them
        if (favoriteShows.Count > 0)
        {
            favoriteShows.ToList().ForEach(async oldFavorite =>
            {
                if (!Podcasts.Any(newPodcast => newPodcast.Url == oldFavorite.Url))
                {

                    await App.PositionData.DeleteFavorite(oldFavorite);
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        FavoriteShows.Remove(oldFavorite);
                    });
                }
            });
        }
    }
}
