// Licensed to the .NET Foundation under one or more agreements.
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
        var item = GetPodcastList();
        item.ForEach(async x =>
        {
            var temp = await FeedService.GetFeed(x);
            podcasts.Add(temp);
        });
        return Task.FromResult(podcasts);
    }

    /// <summary>
    /// Get OPML file from web and return list of current Podcasts.
    /// </summary>
    /// <returns><see cref="List{T}"/> <see cref="string"/> of Url's</returns>
    private static List<string> GetPodcastList()
    {
        List<string> list = new();
        try
        {
            var item = "https://feeds.twit.tv/twitshows_video_hd.opml";
            var reader = new XmlTextReader(item);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        while (reader.MoveToNextAttribute()) // Read the attributes.
                        {
                            if (reader.Name == "xmlUrl")
                            {
                                list.Add(reader.Value);
                            }
                        }
                        break;
                }
            }
            return list;
        }
        catch
        {
            return list;
        }
    }
    #endregion

    #region Update Podcast list
    public static async Task<List<Podcast>> UpdatePodcast()
    {
        var podcasts = await App.PositionData.GetAllPodcasts();
        await App.PositionData.DeleteAllPodcasts();

        // list of stale podcasts
        var stalePodcasts = podcasts.Where(x => x.Deleted).ToList();

        var newPodcasts = await RemoveStalePodcastsAsync(stalePodcasts);

        return await AddPodcastsToDBAsync(stalePodcasts, newPodcasts);
    }
    private static async Task<List<Podcast>> RemoveStalePodcastsAsync(List<Podcast> stalePodcasts)
    {
        // get updated podcast list
        var newPodcasts = await GetFromUrl();
        if (stalePodcasts.Count == 0)
        {
            return newPodcasts;
        }

        // remove stale podcasts
        newPodcasts.ForEach(x =>
        {
            if (!stalePodcasts.Exists(y => y.Deleted == x.Deleted))
            {
                newPodcasts.Remove(x);
                Debug.WriteLine($"Removed stale podcast: {x.Title}");
            }
        });
        return newPodcasts;
    }
    private static async Task<List<Podcast>> AddPodcastsToDBAsync(List<Podcast> stalePodcasts, List<Podcast> newPodcasts)
    {
        var res = new List<Podcast>();
        if (stalePodcasts.Count == 0)
        {
            newPodcasts.ForEach(res.Add);
            Debug.WriteLine("Did not find any stale Podcasts");
        }
        else
        {
            Debug.WriteLine("Found stale podcasts");

            // add all podcasts that are not stale, add all new podcasts if any
            newPodcasts?.ForEach(x =>
            {
                if (!stalePodcasts.Any(y => y.Title == x.Title))
                {
                    res.Add(x);
                    Debug.WriteLine($"Added new podcast: {x.Title}");
                }
            });
        }

        // sort podcast alphabetically
        res = res.OrderBy(x => x.Title).ToList();

        await AddToDatabase(res);
        return res;
    }
    public static async Task<List<Favorites>> UpdateFavorites()
    {
        // get old favorites list
        var favoriteShows = await App.PositionData.GetAllFavorites();
        var podcasts = await App.PositionData.GetAllPodcasts();
        var temp = favoriteShows;

        // if favorite podcasts are stale remove them
        if (favoriteShows.Count == 0)
        {
            Debug.WriteLine("Did not find any stale Favorite Shows");
            return temp;
        }

        favoriteShows.ToList().ForEach(async oldFavorite =>
        {
            if (!podcasts.Any(newPodcast => newPodcast.Url == oldFavorite.Url))
            {
                await App.PositionData.DeleteFavorite(oldFavorite);
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    temp.Remove(oldFavorite);
                });
                Debug.WriteLine($"Removed stale Favorites show: {oldFavorite.Title}");
            }
        });
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
    /// Method Adds a <see cref="Podcast"/> to Database.
    /// </summary>
    /// <param name="url"><see cref="string"/> Url of <see cref="Podcast"/></param>
    /// <returns>nothing</returns>
    public static async Task AddPodcast(string url)
    {
        var podcast = await FeedService.GetFeed(url);
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
