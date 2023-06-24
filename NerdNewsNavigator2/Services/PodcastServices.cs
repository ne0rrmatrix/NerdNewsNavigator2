﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;

/// <summary>
/// A Class for managing Podcasts.
/// </summary>
public static class PodcastServices
{
    #region Properties
    public static bool IsConnected { get; set; } = true;
    #endregion

    #region Get Podcasts

    /// <summary>
    /// Method Retrieves <see cref="List{T}"/> <see cref="Podcast"/> from default RSS Feeds.
    /// </summary>
    /// <returns><see cref="List{T}"/> <see cref="Podcast"/></returns>
    public static Task<List<Podcast>> GetFromUrl()
    {
        List<Podcast> podcasts = new();
        var item = FeedService.GetPodcastListAsync();
        item.ForEach(x =>
        {
            var temp = FeedService.GetFeed(x);
            podcasts.Add(temp);
        });
        return Task.FromResult(podcasts);
    }

    #endregion

    #region Update Podcast list
    public static async Task<List<Podcast>> UpdatePodcast()
    {
        var podcasts = await App.PositionData.GetAllPodcasts();
        var stalePodcasts = new List<Podcast>();

        // list of stale podcasts
        podcasts?.Where(x => x.Deleted).ToList().ForEach(stalePodcasts.Add);
        var newPodcasts = await RemoveStalePodcastsAsync(stalePodcasts);
        return await AddPodcastsToDBAsync(newPodcasts);
    }
    private static async Task<List<Podcast>> RemoveStalePodcastsAsync(List<Podcast> stalePodcasts)
    {
        // get updated podcast list
        var newPodcasts = await GetFromUrl();

        if (stalePodcasts.Count == 0 || stalePodcasts is null)
        {
            Debug.WriteLine("Did not find any deleted podcasts");
            return newPodcasts;
        }

        // on new podcast list mark all old deleted podcast as deleted. If old podcast does not exist it ignores it.
        newPodcasts.ForEach(x =>
        {
            if (stalePodcasts.Exists(y => y.Url == x.Url))
            {
                x.Deleted = true;
                Debug.WriteLine($"marking deleted {x.Title} in new Podcasts");
            }
        });
        return newPodcasts;
    }
    private static async Task<List<Podcast>> AddPodcastsToDBAsync(List<Podcast> newPodcasts)
    {
        var res = new List<Podcast>();
        await App.PositionData.DeleteAllPodcasts();

        // add all podcasts
        newPodcasts.ForEach(res.Add);

        await AddToDatabase(res);
        return res;
    }
    public static async Task<List<Favorites>> UpdateFavoritesAsync()
    {
        // get old favorites list
        var favoriteShows = await App.PositionData.GetAllFavorites();
        var podcasts = await App.PositionData.GetAllPodcasts();
        var temp = new List<Favorites>();

        if (favoriteShows.Count == 0)
        {
            Debug.WriteLine("Did not find any stale Favorite Shows");
            return favoriteShows;
        }
        favoriteShows.ToList().ForEach(oldFavorite =>
        {
            var stale = podcasts.FirstOrDefault(x => x.Title == oldFavorite.Title);
            // if favorite podcasts is not stale add it back to favorites list
            if (stale is not null)
            {
                temp.Add(oldFavorite);
            }
            else
            {
                Debug.WriteLine($"Removed stale Favorites show: {oldFavorite.Title}");
            }
        });
        await App.PositionData.DeleteAllFavorites();
        await AddFavoritesToDatabase(temp);
        return temp;
    }
    #endregion

    #region Manipulate Database

    /// <summary>
    /// Method Adds Playback <see cref="Podcast"/> to Database.
    /// </summary>
    /// <param name="podcast"></param> Position Class object.
    /// <returns>nothing</returns>
    public static async Task AddToDatabase(List<Podcast> podcast)
    {
        foreach (var item in podcast)
        {
            await App.PositionData.AddPodcast(item);
        }
    }

    /// <summary>
    /// Method Adds Playback <see cref="Favorites"/> to Database.
    /// </summary>
    /// <param name="favorite"></param> Position Class object.
    /// <returns>nothing</returns>
    public static async Task AddFavoritesToDatabase(List<Favorites> favorite)
    {
        foreach (var item in favorite)
        {
            await App.PositionData.AddFavorites(item);
        }
    }
    /// <summary>
    /// Method Adds a <see cref="Podcast"/> to Database.
    /// </summary>
    /// <param name="url"><see cref="string"/> Url of <see cref="Podcast"/></param>
    /// <returns>nothing</returns>
    public static async Task AddPodcast(string url)
    {
        var podcast = FeedService.GetFeed(url);
        if (podcast == null)
        {
            return;
        }
        await App.PositionData.AddPodcast(new Podcast
        {
            Title = podcast.Title,
            Url = podcast.Url,
            Download = false,
            IsNotDownloaded = true,
            Description = podcast.Description,
            Image = podcast.Image,
        });
    }

    /// <summary>
    /// Method Adds default <see cref="List{T}"/> <see cref="Podcast"/> from RSS feed to Database.
    /// </summary>
    /// <returns>nothing</returns>
    public static async Task AddDefaultPodcasts()
    {
        await Task.Run(async () =>
        {
            await RemoveDefaultPodcasts();
            var items = await GetFromUrl();
            var res = items.OrderBy(x => x.Title).ToList();
            await AddToDatabase(res);
        });
    }

    /// <summary>
    /// Method resets <see cref="List{T}"/> <see cref="Podcast"/> to default list.
    /// </summary>
    /// <returns>nothing</returns>
    public static async Task RemoveDefaultPodcasts()
    {
        await Task.Run(async () =>
        {
            var items = await App.PositionData.GetAllPodcasts();
            if (items is null || items.Count == 0)
            {
                return;
            }
            items.Where(x => x.Url.Contains("feeds.twit.tv")).ToList().ForEach(async item =>
            {
                await App.PositionData.DeletePodcast(item);
            });
        });
    }

    /// <summary>
    /// Method Deletes a <see cref="Podcast"/> from Database.
    /// </summary>
    /// <param name="url"><see cref="string"/> URL of <see cref="Podcast"/> to delete</param>
    /// <returns><see cref="bool"/> Return True if <see cref="Podcast"/> is Deleted. False Otherwise.</returns>
    public static async Task<bool> Delete(string url)
    {
        var items = await App.PositionData.GetAllPodcasts();
        if (items == null || items.Count == 0)
        {
            return false;
        }
        items.Where(x => x.Url == url).ToList().ForEach(async item =>
        {
            await App.PositionData.DeletePodcast(item);
        });
        return true;
    }
    #endregion
}
