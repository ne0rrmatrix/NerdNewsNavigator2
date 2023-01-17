﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using CodeHollow.FeedReader;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NerdNewsNavigator2.Model;
using NerdNewsNavigator2.View;

namespace NerdNewsNavigator2.ViewModel;

public partial class PodcastViewModel : ObservableObject
{
    #region Properties
    public ObservableCollection<Podcast> Podcasts { get; set; } = new();
    #endregion
    public PodcastViewModel()
    {
        this.Podcasts = PodcastViewModel.GetFeed();
        OnPropertyChanged(nameof(Podcasts));
    }

    private static ObservableCollection<Podcast> GetFeed()
    {
        var numberOfPodcasts = 0;
        ObservableCollection<Podcast> temp = new();
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

        try
        {
            foreach (var url in item)
            {
                var feed = FeedReader.ReadAsync(url);
                Podcast podcasts = new()
                {
                    Title = feed.Result.Title,
                    Description = feed.Result.Description,
                    Image = feed.Result.ImageUrl,
                    Url = url
                };
                temp.Add(podcasts);
                numberOfPodcasts++;
            }
        }
        catch
        {
            Podcast podcats = new()
            {
                Title = string.Empty,
            };
            temp.Add(podcats);
        }
        return temp;
    }

    [RelayCommand]
    async Task Tap(string Url) => await Shell.Current.GoToAsync($"{nameof(ShowPage)}?Url={Url}");
}

