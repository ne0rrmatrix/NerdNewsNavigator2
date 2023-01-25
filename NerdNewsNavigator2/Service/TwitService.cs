// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;
public class TwitService
{
    #region Properties
    List<Podcast> Podcasts { get; } = new();

    readonly List<string> _twit = new()
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
    #region Get the Podcasts
    public static Task<List<string>> GetListOfPodcasts()
    {
        TwitService twitService = new TwitService();
        return Task.FromResult(twitService._twit);
    }
    public Task<List<Podcast>> GetPodcasts(string url)
    {
        if (Podcasts?.Count > 0)
        {
            return Task.FromResult(Podcasts);
        }

        List<Podcast> result = new();
        List<FeedService> feeds = new();

        var feed = FeedService.GetFeed(url);
        feeds.Add(feed);
        foreach (var show in feed._podcasts)
        {
            result.Add(show);
        }

        return Task.FromResult(result);
        #endregion
    }
    #region Get Shows
    public static Task<List<Show>> GetShow(string url)
    {
        var result = new List<Show>();
        try
        {
            var feed = FeedService.GetShow(url);
            foreach (var items in feed._podcasts)
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
