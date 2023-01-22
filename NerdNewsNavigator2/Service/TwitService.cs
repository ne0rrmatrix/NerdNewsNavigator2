// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;
public class TwitService
{
    List<Podcast> podcasts { get; } = new();
    public TwitService()
    {
    }
    public async Task<List<Podcast>> GetPodcasts()
    {
        if (podcasts?.Count > 0)
            return podcasts;

        podcasts.Clear();
        var numberOfPodcasts = 0;
        List<Podcast> result = new();
        #region List of podcats
        List<string> item = new()
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
        try
        {
            foreach (var url in item)
            {
                var feed = await FeedReader.ReadAsync(url);
                Podcast items = new()
                {
                    Title = feed.Title,
                    Description = feed.Description,
                    Image = feed.ImageUrl,
                    Url = url
                };
                result.Add(items);
                numberOfPodcasts++;
            }
        }
        catch
        {
            Podcast items = new()
            {
                Title = string.Empty,
            };
            result.Add(items);
        }
        return result;
    }
    public async Task<List<Show>> GetShow(string url)
    {
        List<Show> result = new();
        try
        {
            var feed = await FeedReader.ReadAsync(url);
            foreach (var item in feed.Items)
            {
                Show show = new()
                {
                    Title = item.Title,
                    Description = item.Description,
                    Image = item.GetItunesItem().Image.Href,
                    Url = item.Id
                };
                result.Add(show);
            }
            return result;
        }
        catch
        {
            Show show = new()
            {
                Title = string.Empty,
            };
            result.Add(show);
            return result;
        }
    }
}

