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
