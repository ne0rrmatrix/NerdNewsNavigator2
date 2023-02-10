// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NerdNewsNavigator2.Model;

namespace NerdNewsNavigator2.Service;
public class TwitService : ObservableObject
{
    public List<Podcast> Podcasts { get; set; } = new();
    public FileService File { get; set; }
    public TwitService(FileService fileService)
    {
        File = fileService;
        Podcasts.Clear();
        Podcasts = GetListOfPodcasts().Result;
    }
    public static async Task<List<Podcast>> GetListOfPodcasts()
    {
        List<Podcast> podcasts = new List<Podcast>();
        var fileService = new FileService();
        var podcastList = fileService.GetJsonData();
        foreach (var item in podcastList)
        {
            var temp = await Task.FromResult(FeedService.GetFeed(item));
            podcasts.Add(temp);
        }
        return podcasts;
    }
    public Task<List<Podcast>> GetData()
    {
        return Task.FromResult(Podcasts);
    }
    public static Task<List<Show>> GetShow(string url)
    {
        return Task.FromResult(FeedService.GetShow(url));
    }
}
