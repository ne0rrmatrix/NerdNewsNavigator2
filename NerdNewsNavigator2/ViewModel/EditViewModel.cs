// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="EditPage"/>
/// </summary>
public partial class EditViewModel : SharedViewModel
{
    /// <summary>
    /// An <see cref="ILogger"/> instance managed by this class.
    /// </summary>
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(EditViewModel));
    /// <summary>
    /// Initializes a new instance of the <see cref="EditViewModel"/> instance.
    /// </summary>
    public EditViewModel(IConnectivity connectivity) : base(connectivity)
    {
        ThreadPool.QueueUserWorkItem(async (state) => await GetUpdatedPodcasts());
    }

    #region Methods
    /// <summary>
    /// Method checks for required Permission for Android Notifications and requests them if needed
    /// </summary>
    /// <returns></returns>
    public static async Task<PermissionStatus> CheckAndRequestForeGroundPermission()
    {
        var status = await Permissions.CheckStatusAsync<AndroidPermissions>();
        if (status == PermissionStatus.Granted)
        {
            return status;
        }
        else
        {
            await Shell.Current.DisplayAlert("Permission Required", "Notification permission is required for Auto Downloads to work in background. It runs on an hourly schedule.", "Ok");
        }
        status = await Permissions.RequestAsync<AndroidPermissions>();
        return status;
    }
    private async Task DeleteFavorites(List<Favorites> favoriteShow, string url)
    {
        var item = favoriteShow.ToList().Exists(x => x.Url == url);
        if (item)
        {
            await FavoriteService.RemoveFavoriteFromDatabase(url);
            var fav = FavoriteShows.First(x => x.Url == url);
            favoriteShow?.Remove(fav);
        }
    }
    private bool SetPreferences()
    {
        var start = Preferences.Default.Get("start", false);
        if (start)
        {
            _logger.Info("Auto Download is already set to start Automatically");
            return true;
        }
        return true;
    }
    private async Task ProcessPodcastsAsync(Podcast item)
    {
        item.Download = true;
        item.IsNotDownloaded = false;
        Podcasts[Podcasts.IndexOf(item)] = item;

        await App.PositionData.UpdatePodcast(item);
    }
    private async Task ProcessFavoritesAsync(Podcast item)
    {
        Favorites favorite = new()
        {
            Title = item.Title,
            Url = item.Url,
            Description = item.Description,
            Image = item.Image,
            PubDate = item.PubDate,
        };
        FavoriteShows.Add(favorite);
        await FavoriteService.AddFavoriteToDatabase(favorite);
    }
    #endregion

    #region Events
    /// <summary>
    /// Method Deletes a Podcast from the database.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task DeletePodcast(string url)
    {
        var exists = Podcasts.ToList().Exists(x => x.Url == url);
        if (exists)
        {
            var podcast = Podcasts.First(x => x.Url == url);
            Podcasts?.Remove(podcast);
            podcast.Deleted = true;
            await App.PositionData.UpdatePodcast(podcast);
        }
        var favoriteShow = await App.PositionData.GetAllFavorites();
        if (favoriteShow is null || favoriteShow.Count == 0)
        {
            return;
        }
        await DeleteFavorites(favoriteShow, url);
    }

    /// <summary>
    /// A Method that adds a favourite to the database.
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task<bool> AddToFavorite(string url)
    {
        var status = await CheckAndRequestForeGroundPermission();
        if (PermissionStatus.Granted == status)
        {
            _logger.Info("Notification Permission Granted");
        }
        else if (PermissionStatus.Denied == status)
        {
            _logger.Info("Notification Permission Denied");
        }
        if (FavoriteShows.AsEnumerable().Any(x => x.Url == url))
        {
            return false;
        }
        else if (Podcasts.AsEnumerable().Any(x => x.Url == url))
        {
            var item = Podcasts.First(x => x.Url == url);
            await ProcessFavoritesAsync(item);
            await ProcessPodcastsAsync(item);

            return SetPreferences();
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
        if (!FavoriteShows.AsEnumerable().Any(x => x.Url == url))
        {
            return false;
        }

        var fav = FavoriteShows.First(x => x.Url == url);
        FavoriteShows.Remove(FavoriteShows[FavoriteShows.IndexOf(fav)]);

        var item = Podcasts.First(x => x.Url == url);
        if (item is not null)
        {
            item.Download = false;
            item.IsNotDownloaded = true;
            Podcasts[Podcasts.IndexOf(item)] = item;
            await App.PositionData.UpdatePodcast(item);
        }

        await FavoriteService.RemoveFavoriteFromDatabase(url);
        return true;
    }
    #endregion
}
