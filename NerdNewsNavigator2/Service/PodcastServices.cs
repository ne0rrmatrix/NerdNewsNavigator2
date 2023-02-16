// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;
public partial class PodcastServices
{
    private readonly List<string> _twit = new()
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
    private List<Podcast> Current { get; set; } = new();
    public PodcastServices()
    {
        _ = GetUpdatedPodcasts();
    }
    ~PodcastServices()
    {
        Current.Clear();
    }
    public async Task GetUpdatedPodcasts()
    {
        Current.Clear();
        var temp = await App.PositionData.GetAllPodcasts();
        foreach (var item in temp)
        {
            Current.Add(item);
        }
    }
    public Task<List<Podcast>> GetAllPodcasts()
    {
        return Task.FromResult(Current);
    }
    public async Task AddToDatabase()
    {
        foreach (var item in Current)
        {
            await App.PositionData.AddPodcast(item);
            Current.Add(item);
        }
    }
    public async Task<List<Podcast>> GetFromUrl()
    {
        List<Podcast> podcasts = new();
        foreach (var item in _twit)
        {
            var temp = await Task.FromResult(FeedService.GetFeed(item));
            podcasts.Add(temp);
            Current.Add(temp);
        }

        return podcasts;
    }
    public async Task RemoveDefaultPodcasts()
    {
        foreach (var item in Current)
        {
            if (item.Url.Contains("feeds.twit.tv"))
            {
                await App.PositionData.DeletePodcast(item);
            }
        }
        Current.Clear();
    }
    public async Task AddDefaultPodcasts()
    {
        foreach (var item in Current)
        {
            if (item.Url.Contains("feeds.twit.tv"))
            {
                await App.PositionData.DeletePodcast(item);
                Current.Remove(item);
            }
        }
        var items = GetFromUrl().Result;
        foreach (var item in items)
        {
            Current.Add(item);
            await App.PositionData.AddPodcast(item);
        }
    }
    public async Task DeleteAll()
    {
        await App.PositionData.DeleteAllPodcasts();
        Current.Clear();
        _ = GetFromUrl().Result;
    }
    public static Task<List<Show>> GetShow(string url)
    {
        return Task.FromResult(FeedService.GetShow(url));
    }
    public async Task<bool> SaveAll(List<Podcast> podcasts)
    {
        foreach (var item in podcasts)
        {
            await AddPodcast(item.Url);
        }
        return true;
    }
    public async Task AddPodcast(string url)
    {
        var podcast = await Task.FromResult(FeedService.GetFeed(url));
        await App.PositionData.AddPodcast(new Podcast
        {
            Title = podcast.Title,
            Url = podcast.Url,
            Description = podcast.Description,
            Image = podcast.Image,
        });
    }
    public async Task<bool> Delete(string url)
    {
        foreach (var item in from item in Current
                             where item.Url == url
                             select item)
        {
            if (Current.Contains(item)) { Current.Remove(item); }

            await App.PositionData.DeletePodcast(item);
            break;
        }

        return true;
    }
}
