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
            "https://feeds.twit.tv/leo_video_hd.xml",
            "https://feeds.twit.tv/ant_video_hd.xml",
            "https://feeds.twit.tv/jason_video_hd.xml",
            "https://feeds.twit.tv/mikah_video_hd.xml",
            "https://feeds.twit.tv/twis_video_hd.xml",
            "https://feeds.twit.tv/uls_video_hd.xml",
            "https://feeds.twit.tv/htg_video_hd.xml",
            "https://feeds.twit.tv/floss_video_hd.xml"
        };
    #endregion

    /// <summary>
    /// Method Retrieves <see cref="List{T}"/> <see cref="Podcast"/> from default RSS Feeds.
    /// </summary>
    /// <returns><see cref="List{T}"/> <see cref="Podcast"/></returns>
    public static Task<List<Podcast>> GetFromUrl()
    {
        List<Podcast> podcasts = new();
        s_twit.ForEach(async podcast =>
        {
            var temp = await FeedService.GetFeed(podcast);
            podcasts.Add(temp);
        });
        return Task.FromResult(podcasts);
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
    /// Method Adds a <see cref="Podcast"/> to Database.
    /// </summary>
    /// <param name="url"><see cref="string"/> Url of <see cref="Podcast"/></param>
    /// <returns>nothing</returns>
    public static async Task AddPodcast(string url)
    {
        var podcast = await Task.FromResult(FeedService.GetFeed(url)).Result;
        if (podcast == null)
        {
            return;
        }
        await App.PositionData.AddPodcast(new Podcast
        {
            Title = podcast.Title,
            Url = podcast.Url,
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
        await RemoveDefaultPodcasts();
        var items = await GetFromUrl();
        var res = items.OrderBy(x => x.Title).ToList();
        await AddToDatabase(res);
    }

    /// <summary>
    /// Method resets <see cref="List{T}"/> <see cref="Podcast"/> to default list.
    /// </summary>
    /// <returns>nothing</returns>
    public static async Task RemoveDefaultPodcasts()
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
