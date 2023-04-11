// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;

/// <summary>
/// A Class for managing Podcasts.
/// </summary>
public static class PodcastServices
{
    #region Properties

    /// <summary>
    /// Default URL <see cref="List{T}"/> <see cref="string"/> for Twit podcasts.
    /// </summary>

    private static readonly List<string> s_twit = new()
        {
            "https://feeds.twit.tv/ww_video_hd.xml",
            "https://feeds.twit.tv/aaa_video_hd.xml",
            "https://feeds.twit.tv/hom_video_hd.xml",
            "https://feeds.twit.tv/hop_video_hd.xml",
            "https://feeds.twit.tv/howin_video_hd.xml",
            "https://feeds.twit.tv/ipad_video_hd.xml",
            "https://feeds.twit.tv/mbw_video_hd.xml",
            "https://feeds.twit.tv/sn_video_hd.xml",
            "https://feeds.twit.tv/ttg_video_hd.xml",
            "https://feeds.twit.tv/tnw_video_hd.xml",
            "https://feeds.twit.tv/twiet_video_hd.xml",
            "https://feeds.twit.tv/twig_video_hd.xml",
            "https://feeds.twit.tv/twit_video_hd.xml",
            "https://feeds.twit.tv/events_video_hd.xml",
            "https://feeds.twit.tv/specials_video_hd.xml",
            "https://feeds.twit.tv/bits_video_hd.xml",
            "https://feeds.twit.tv/throwback_video_large.xml",
            "https://feeds.twit.tv/leo_video_hd.xml",
            "https://feeds.twit.tv/ant_video_hd.xml",
            "https://feeds.twit.tv/jason_video_hd.xml",
            "https://feeds.twit.tv/mikah_video_hd.xml"
        };
    #endregion

    /// <summary>
    /// Method Gets updated <see cref="List{T}"/> <see cref="Podcast"/> from Database.
    /// </summary>
    /// <returns><see cref="List{T}"/> <see cref="Podcast"/></returns>
    public static async Task<List<Podcast>> GetUpdatedPodcasts()
    {
        var temp = await App.PositionData.GetAllPodcasts();
        return temp;
    }

    /// <summary>
    /// Method Adds Playback <see cref="Position"/> to Database.
    /// </summary>
    /// <param name="position"></param> Position Class object.
    /// <returns>nothing</returns>
    public static async Task AddToDatabase(List<Podcast> position)
    {
        foreach (var item in position)
        {
            await App.PositionData.AddPodcast(item);
        }
    }

    /// <summary>
    /// Method Retrieves <see cref="List{T}"/> <see cref="Podcast"/> from default RSS Feeds.
    /// </summary>
    /// <returns><see cref="List{T}"/> <see cref="Podcast"/></returns>
    public static async Task<List<Podcast>> GetFromUrl()
    {
        List<Podcast> podcasts = new();
        foreach (var item in s_twit)
        {
            var temp = await FeedService.GetFeed(item);
            podcasts.Add(temp);
        }
        return podcasts;
    }

    /// <summary>
    /// Method resets <see cref="List{T}"/> <see cref="Podcast"/> to default list.
    /// </summary>
    /// <returns>nothing</returns>
    public static async Task RemoveDefaultPodcasts()
    {
        var current = await App.PositionData.GetAllPodcasts();
        foreach (var item in from item in current
                             where item.Url.Contains("feeds.twit.tv")
                             select item)
        {
            await App.PositionData.DeletePodcast(item);
        }
    }

    /// <summary>
    /// Method Adds default <see cref="List{T}"/> <see cref="Podcast"/> from RSS feed to Database.
    /// </summary>
    /// <returns>nothing</returns>
    public static async Task AddDefaultPodcasts()
    {
        await RemoveDefaultPodcasts();

        var items = GetFromUrl().Result;
        if (items is not null)
        {
            foreach (var item in items)
            {
                await App.PositionData.AddPodcast(item);
            }
        }
    }

    /// <summary>
    /// Method Removes <see cref="List{T}"/> <see cref="Podcast"/> from Database.
    /// </summary>
    /// <returns>nothing</returns>
    public static async Task DeleteAll()
    {
        await App.PositionData.DeleteAllPodcasts();
    }

    /// <summary>
    /// Method Returns a <see cref="Show"/> from RSS Feed.
    /// </summary>
    /// <param name="url">The <see cref="string"/> URL of the Show.</param> 
    /// <param name="getFirstOnly">The <see cref="bool"/> Get First value only.</param>
    /// <returns><see cref="List{T}"/> <see cref="Show"/></returns>
    public static Task<List<Show>> GetShow(string url, bool getFirstOnly)
    {
        return FeedService.GetShows(url, getFirstOnly);
    }

    /// <summary>
    /// Method Saves <see cref="List{T}"/> of <see cref="Podcast"/> to database.
    /// </summary>
    /// <param name="podcasts">the <see cref="List{T}"/>List of Podcasts.</param>
    /// <returns>nothing</returns>
    public static async Task SaveAll(List<Podcast> podcasts)
    {
        foreach (var item in podcasts)
        {
            await AddPodcast(item.Url);
        }
    }

    /// <summary>
    /// Method Adds a <see cref="Podcast"/> to Database.
    /// </summary>
    /// <param name="url"><see cref="string"/> Url of <see cref="Podcast"/></param>
    /// <returns>nothing</returns>
    public static async Task AddPodcast(string url)
    {
        var podcast = await Task.FromResult(FeedService.GetFeed(url)).Result;
        if (podcast != null)
        {
            await App.PositionData.AddPodcast(new Podcast
            {
                Title = podcast.Title,
                Url = podcast.Url,
                Description = podcast.Description,
                Image = podcast.Image,
            });
        }
    }

    /// <summary>
    /// Method Deletes a <see cref="Podcast"/> from Database.
    /// </summary>
    /// <param name="url"><see cref="string"/> URL of <see cref="Podcast"/> to delete</param>
    /// <returns>nothing</returns>
    public static async Task<bool> Delete(string url)
    {
        var current = await App.PositionData.GetAllPodcasts();
        foreach (var item in current)
        {
            if (item.Url == url)
            {
                try
                {
                    await App.PositionData.DeletePodcast(item);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        return true;
    }
    /// <summary>
    /// Method Updates a <see cref="Podcast"/> to Database.
    /// </summary>
    /// <param name="podcast"><see cref="string"/> Url of <see cref="Podcast"/></param>
    /// <returns>nothing</returns>
    public static async Task UpdatePodcast(Podcast podcast)
    {
        await App.PositionData.UpdatePodcast(podcast);
    }
}
