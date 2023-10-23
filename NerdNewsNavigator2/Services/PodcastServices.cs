// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;

/// <summary>
/// A Class for managing Podcasts.
/// </summary>
public static class PodcastServices
{
    /// <summary>
    /// Method Retrieves <see cref="List{T}"/> <see cref="Podcast"/> from default RSS Feeds.
    /// </summary>
    /// <returns><see cref="List{T}"/> <see cref="Podcast"/></returns>
    public static List<Podcast> GetFromUrl()
    {
        List<Podcast> podcasts = new();
        var item = FeedService.GetPodcastListAsync();
        item.ForEach(x => podcasts.Add(FeedService.GetFeed(x)));
        return podcasts;
    }

    #region Manipulate Database

    /// <summary>
    /// Method Adds Playback <see cref="Podcast"/> to Database.
    /// </summary>
    /// <param name="podcast"></param> Position Class object.
    /// <returns>nothing</returns>
    public static async Task AddToDatabase(List<Podcast> podcast)
    {
        for (var i = 0; i < podcast.Count; i++)
        {
            Position pos = new()
            {
                Title = podcast[i].Title,
                Description = podcast[i].Description,
                Url = podcast[i].Url,
                Image = podcast[i].Image,
                Download = podcast[i].Download,
                Link = podcast[i].Link,
                PubDate = podcast[i].PubDate
            };
            await App.PositionData.AddPosition(pos);
            await App.PositionData.AddPodcast(podcast[i]);
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
    public static void AddDefaultPodcasts()
    {
        ThreadPool.QueueUserWorkItem(async state =>
        {
            await RemoveDefaultPodcasts();
            var items = GetFromUrl();
            var res = items.OrderBy(x => x.Title).ToList();
            foreach (var item in res)
            {
                await App.PositionData.AddPodcast(item);
            }
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
            items.Where(x => x.Url.Contains("feeds.twit.tv")).ToList().ForEach(async item => await App.PositionData.DeletePodcast(item));
        });

    }
    #endregion
}
