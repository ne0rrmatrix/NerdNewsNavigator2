// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;
public class TwitService
{
    #region Properties
    List<Podcast> Podcasts { get; } = new();
    #endregion
    #region List of podcats
    public Task<List<Podcast>> GetPodcasts()
    {
        if (Podcasts?.Count > 0)
            return Task.FromResult(Podcasts);

        List<Podcast> result = new();
        List<string> twit = new()
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
        #region GetPodcasts
        List<FeedService> feeds = new();
        FeedService feed = new();
        var counter = 0;
        foreach (var item in twit)
        {
            feed = feed.GetFeed(item);
            feeds.Add(feed);
            foreach (var show in feed.Podcasts)
            {
                result.Add(show);
            }
            counter++;
        }
        return Task.FromResult(result);
        #endregion
    }
    #region Get Shows
    public static Task<List<Show>> GetShow(string url)
    {
        var result = new List<Show>();
        FeedService feed = new();
        try
        {
            feed = feed.GetShow(url);
            foreach (var items in feed.Podcasts)
            {
                return Task.FromResult(items.GetShows());
            }
        }
        catch
        {
            Show show = new()
            {
                Title = string.Empty,
            };
            result.Add(show);
        }
        return Task.FromResult(result);
        #endregion
    }
}
