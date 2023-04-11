// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="RemovePage"/>
/// </summary>
public partial class RemoveViewModel : BaseViewModel
{
    ILogger<RemoveViewModel> Logger { get; set; }
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveViewModel"/> instance.
    /// </summary>
    public RemoveViewModel(ILogger<RemoveViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        Logger = logger;
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        Orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
        _ = GetUpdatedPodcasts();
    }
    /// <summary>
    /// Method Deletes a Podcast from the database.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task DeletePodcast(string url)
    {
        if (Podcasts.AsEnumerable().Any(x => x.Url == url))
        {
            var item = Podcasts.First(x => x.Url == url);

            await FavoriteService.RemoveFavoriteFromDatabase(url);
            ThreadPool.QueueUserWorkItem(GetFavoriteShows);

            await PodcastServices.Delete(url);
            Podcasts.Remove(item);
            Logger.LogInformation("Removed show {item} from database", item.Url);
            await GetUpdatedPodcasts();
        }
    }

    /// <summary>
    /// A Method that adds a favourite to the database.
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task<bool> AddToFavorite(string url)
    {
        if (FavoriteShows.AsEnumerable().Any(x => x.Url == url))
        {
            return false;
        }
        else if (Podcasts.AsEnumerable().Any(x => x.Url == url))
        {
            var item = Podcasts.First(x => x.Url == url);
            var show = new Show
            {
                Url = item.Url,
                Title = item.Title,
                Description = item.Description,
                Image = item.Image,
            };
            await FavoriteService.AddFavoriteToDatabase(show);
            item.Download = true;
            await PodcastServices.UpdatePodcast(item);
            Logger.LogInformation("Added {item} to database", item.Url);
            await GetUpdatedPodcasts();
            ThreadPool.QueueUserWorkItem(GetFavoriteShows);
            ThreadPool.QueueUserWorkItem(App.AutoDownload);
            return true;
        }
        return false;
    }

    /// <summary>
    /// A Method that removes a favourite from the database.
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task<bool> RemoveFavorite(string url)
    {
        if (FavoriteShows.AsEnumerable().Any(x => x.Url == url))
        {
            return false;
        }
        await FavoriteService.RemoveFavoriteFromDatabase(url);
        var item = Podcasts.First(x => x.Url == url);
        item.Download = false;
        await PodcastServices.UpdatePodcast(item);
        await PodcastServices.GetUpdatedPodcasts();
        Logger.LogInformation("Removed {item} from database", item.Url);
        Podcasts = new ObservableCollection<Podcast>(Podcasts);
        OnPropertyChanged(nameof(Podcasts));
        ThreadPool.QueueUserWorkItem(GetFavoriteShows);
        return true;
    }
}
