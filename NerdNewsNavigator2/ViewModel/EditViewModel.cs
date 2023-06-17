// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="EditPage"/>
/// </summary>
public partial class EditViewModel : BaseViewModel
{
    private readonly IMessenger _messenger;
    /// <summary>
    /// An <see cref="ILogger{TCategoryName}"/> instance managed by this class.
    /// </summary>
    ILogger<EditViewModel> Logger { get; set; }
    /// <summary>
    /// Initializes a new instance of the <see cref="EditViewModel"/> instance.
    /// </summary>
    public EditViewModel(ILogger<EditViewModel> logger, IConnectivity connectivity, IMessenger messenger) : base(logger, connectivity)
    {
        Logger = logger;
        _messenger = messenger;
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        Orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
        _ = GetUpdatedPodcasts();
        if (DownloadService.IsDownloading)
        {
            ThreadPool.QueueUserWorkItem(state => { UpdatingDownload(); });
        }
    }
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
        }

        var favoriteShow = await App.PositionData.GetAllFavorites();
        if (favoriteShow is null || favoriteShow.Count == 0)
        {
            return;
        }

        var item = favoriteShow.ToList().Exists(x => x.Url == url);
        if (item)
        {
            await FavoriteService.RemoveFavoriteFromDatabase(url);
            var fav = FavoriteShows.First(x => x.Url == url);
            favoriteShow?.Remove(fav);
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
#if ANDROID
        var status = await CheckAndRequestForeGroundPermission();
        if (PermissionStatus.Granted == status)
        {
            Logger.LogInformation("Notification Permission Granted");
        }
        else if (PermissionStatus.Denied == status)
        {
            Logger.LogInformation("Notification Permission Denied");
        }
#endif
        if (FavoriteShows.AsEnumerable().Any(x => x.Url == url))
        {
            return false;
        }
        else if (Podcasts.AsEnumerable().Any(x => x.Url == url))
        {
            var item = Podcasts.First(x => x.Url == url);
            Favorites favorite = new()
            {
                Title = item.Title,
                Url = item.Url,
                Description = item.Description,
                Image = item.Image,
                PubDate = item.PubDate,
            };
            await FavoriteService.AddFavoriteToDatabase(favorite);
            FavoriteShows.Add(favorite);

            item.Download = true;
            item.IsNotDownloaded = false;
            await PodcastServices.UpdatePodcast(item);
            Podcasts[Podcasts.IndexOf(item)] = item;

            var start = Preferences.Default.Get("start", false);
            if (start)
            {
                Logger.LogInformation("Auto Download is already set to start Automatically");
                return true;
            }
            Preferences.Default.Set("start", true);

            Logger.LogInformation("Setting Auto Download to start Automatically");
            _messenger.Send(new MessageData(true));

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
        if (!FavoriteShows.AsEnumerable().Any(x => x.Url == url))
        {
            return false;
        }

        await FavoriteService.RemoveFavoriteFromDatabase(url);

        var item = Podcasts.First(x => x.Url == url);
        if (item is not null)
        {
            item.Download = false;
            item.IsNotDownloaded = true;
            await PodcastServices.UpdatePodcast(item);
            Podcasts[Podcasts.IndexOf(item)] = item;
        }

        var fav = FavoriteShows.First(x => x.Url == url);
        FavoriteShows.Remove(FavoriteShows[FavoriteShows.IndexOf(fav)]);

        return true;
    }
}
