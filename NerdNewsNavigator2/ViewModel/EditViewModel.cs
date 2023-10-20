// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="EditPage"/>
/// </summary>
public partial class EditViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EditViewModel"/> instance.
    /// </summary>
    public EditViewModel(IConnectivity connectivity) : base(connectivity)
    {
        _ = GetPodcasts();
    }

    #region Methods
    /// <summary>
    /// Method checks for required Permission for Android Notifications and requests them if needed
    /// </summary>
    /// <returns></returns>
    public static async Task CheckAndRequestForeGroundPermission()
    {
        var status = await Permissions.CheckStatusAsync<AndroidPermissions>();
        if (status == PermissionStatus.Granted)
        {
            return;
        }
        else
        {
            await Shell.Current.DisplayAlert("Permission Required", "Notification permission is required for Auto Downloads to work in background. It runs on an hourly schedule.", "Ok");
        }
        await Permissions.RequestAsync<AndroidPermissions>();
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
        var item = Podcasts.ToList().Find(x => x.Url == url);
        Podcasts.Remove(item);
        await App.PositionData.DeletePodcast(item);

        var fav = FavoriteShows.ToList().Find(x => x.Url == url);
        if (fav is null)
        {
            return;
        }
        FavoriteShows.Remove(fav);
        await App.PositionData.DeleteFavorite(fav);
    }

    /// <summary>
    /// A Method that adds a favourite to the database.
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddToFavorite(string url)
    {
        await CheckAndRequestForeGroundPermission();
        var item = Podcasts.ToList().Find(x => x.Url == url);
        var num = Podcasts.IndexOf(item);

        Favorites favorite = new()
        {
            Title = item.Title,
            Url = item.Url,
            Description = item.Description,
            Image = item.Image,
            PubDate = item.PubDate,
            Download = true,
            IsNotDownloaded = false,
        };
        FavoriteShows.Add(favorite);
        await App.PositionData.AddFavorites(favorite);

        item.Download = true;
        item.IsNotDownloaded = false;
        Podcasts[num] = item;
        await App.PositionData.UpdatePodcast(item);
    }

    /// <summary>
    /// A Method that removes a favourite from the database.
    /// </summary>
    /// <param name="url">A Url <see cref="string"/></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task RemoveFavorite(string url)
    {
        var item = Podcasts.ToList().Find(x => x.Url == url);
        var num = Podcasts.IndexOf(item);
        var fav = FavoriteShows.ToList().Find(x => x.Url == url);

        FavoriteShows.Remove(fav);
        await App.PositionData.DeleteFavorite(fav);

        item.IsNotDownloaded = true;
        item.Download = false;
        Podcasts[num] = item;
        await App.PositionData.UpdatePodcast(item);
    }
    #endregion
}
