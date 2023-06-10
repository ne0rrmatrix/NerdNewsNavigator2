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
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        Orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
        _ = GetUpdatedPodcasts();
        if (DownloadService.IsDownloading)
        {
            ThreadPool.QueueUserWorkItem(state => { UpdatingDownload(); });
        }
        _messenger = messenger;
    }

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
        var result = await PodcastServices.Delete(url);
        if (!result)
        {
            return;
        }
        var podcast = Podcasts.First(x => x.Url == url);
        Podcasts.Remove(podcast);
        Logger.LogInformation("Removed show {item} from database", podcast.Url);
        var favoriteShow = await App.PositionData.GetAllFavorites();
        if (favoriteShow is null || favoriteShow.Count == 0)
        {
            return;
        }
        var item = favoriteShow.First(x => x.Url == url);
        if (item is null)
        {
            return;
        }
        await FavoriteService.RemoveFavoriteFromDatabase(url);
        FavoriteShows.Remove(item);
        Logger.LogInformation("Removed Favorite {item} from database", podcast.Url);
        await GetUpdatedPodcasts();
    }
    public void SetData(object stateinfo)
    {
        Preferences.Default.Set("AutoDownload", true);
        _messenger?.Send(new MessageData(true));
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
            Logger.LogInformation("Background service working!");
        }
        else if (PermissionStatus.Denied == status)
        {
            Logger.LogInformation("Failed to add background service");
        }
#endif
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
            Podcasts[Podcasts.IndexOf(item)] = item;
            ThreadPool.QueueUserWorkItem(GetFavoriteShows);
            Podcasts = new ObservableCollection<Podcast>(Podcasts);
            Logger.LogInformation("Added {item} to database", item.Url);
            ThreadPool.QueueUserWorkItem(SetData);
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
        await GetUpdatedPodcasts();
        Logger.LogInformation("Removed {item} from database", item.Url);
        Podcasts.Remove(item);
        ThreadPool.QueueUserWorkItem(GetFavoriteShows);
        return true;
    }
}
