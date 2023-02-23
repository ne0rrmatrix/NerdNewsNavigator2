// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;
/// <summary>
/// Class <c>PodcastService</c> Has methods for processing Podcasts.
/// </summary>
public static class PodcastServices
{
    #region Properties
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
    /// Method <c>GetUpdatedPodcasts</c> Gets updated Podcasts from Database.
    /// </summary>
    /// <returns><see cref="List{T}"/> <see cref="Podcast"/></returns>
    public static async Task<List<Podcast>> GetUpdatedPodcasts()
    {
        var temp = await App.PositionData.GetAllPodcasts();
        return temp;
    }
    /// <summary>
    /// Method <c>AddToDatabase</c> Adds Playback position to Database.
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
    /// Method <c>GetFromUrl</c> Retrieves Podcasts from default RSS Feeds.
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
    /// Method <c>RemoveDefaultPodcasts</c> Clears Database of all default Podcasts.
    /// </summary>
    /// <returns>nothing</returns>
    public static async Task RemoveDefaultPodcasts()
    {
        var current = await App.PositionData.GetAllPodcasts();
        Debug.WriteLine("Got current");
        foreach (var item in current)
        {
            if (item.Url.Contains("feeds.twit.tv"))
            {
                await App.PositionData.DeletePodcast(item);
            }
        }
    }
    /// <summary>
    /// Method <c>AddDefaultPodcasts</c> Adds default Podcasts from RSS feed to Database.
    /// </summary>
    /// <returns>nothing</returns>
    public static async Task AddDefaultPodcasts()
    {
        await RemoveDefaultPodcasts();

        var items = GetFromUrl().Result;
        foreach (var item in items)
        {
            Debug.WriteLine($"Adding Podcast: {item.Title}");
            await App.PositionData.AddPodcast(item);
        }
    }
    /// <summary>
    /// Method <c>DeleteAll</c> Removes all Podcasts from Database.
    /// </summary>
    /// <returns>nothing</returns>
    public static async Task DeleteAll()
    {
        await App.PositionData.DeleteAllPodcasts();
    }
    /// <summary>
    /// Method <c>GetShow</c> Returns a Single Show from RSS Feed.
    /// </summary>
    /// <param name="url">The <see cref="System.String"/>The URL of Show.</param> 
    /// <returns><see cref="List{T}"/> <see cref="Show"/></returns>
    public static Task<List<Show>> GetShow(string url)
    {
        return FeedService.GetShow(url);
    }
    /// <summary>
    /// Method <c>SaveAll</c> Saves List of Podcasts.
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
    /// Method <c>AddPodcast</c> Adds a Podcast to Database.
    /// </summary>
    /// <param name="url"></param> URL string of Podcast.
    /// <returns>nothing</returns>
    public static async Task AddPodcast(string url)
    {
        var podcast = await Task.FromResult(FeedService.GetFeed(url)).Result;
        await App.PositionData.AddPodcast(new Podcast
        {
            Title = podcast.Title,
            Url = podcast.Url,
            Description = podcast.Description,
            Image = podcast.Image,
        });
    }
    /// <summary>
    /// Method <c>Delete</c> Deletes a Podcast from Database.
    /// </summary>
    /// <param name="url">The <see cref="System.String"/>URL of Podcast to delete</param>
    /// <returns>nothing</returns>
    public static async Task Delete(string url)
    {
        var current = await App.PositionData.GetAllPodcasts();
        foreach (var item in current)
        {
            if (item.Url == url)
            {
                await App.PositionData.DeletePodcast(item);
            }
        }
    }
}
