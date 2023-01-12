using CodeHollow.FeedReader;
using CommunityToolkit.Mvvm.ComponentModel;
using NerdNewsNavigator2.Model;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace NerdNewsNavigator2.ViewModel
{
    public partial class PodcastViewModel : ObservableObject
    {
        #region Properties
        public ObservableCollection<Podcast> Podcasts { get; set; } = new ObservableCollection<Podcast>();
        #endregion
        public PodcastViewModel()
        {
            this.Podcasts = GetFeed();
        }

        private ObservableCollection<Podcast> GetFeed()
        {
            int numberOfPodcasts = 0;
            ObservableCollection<Podcast> temp = new ObservableCollection<Podcast>();
            string jsonString = @"[{""title"":""https://feeds.twit.tv/ww_video_hd.xml""},{""title"":""https://feeds.twit.tv/aaa_video_hd.xml""},{""title"":""https://feeds.twit.tv/floss_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/hom_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/hop_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/howin_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/ipad_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/mbw_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/sn_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/ttg_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/tnw_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/twiet_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/twig_video_hd.xml""},{""title"":""https://feeds.twit.tv/twit_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/events_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/specials_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/bits_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/throwback_video_large.xml""},{ ""title"":""https://feeds.twit.tv/leo_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/ant_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/jason_video_hd.xml""},{ ""title"":""https://feeds.twit.tv/mikah_video_hd.xml""}]";
            var data = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonString);

            foreach (var item in data)
            {
                foreach (var (podcast, url) in item)
                {
                    var feed = FeedReader.ReadAsync(url);
                    Podcast podcasts = new Podcast();
                    podcasts.Title = feed.Result.Title;
                    podcasts.Description = feed.Result.Description;
                    podcasts.Image = feed.Result.ImageUrl;
                    podcasts.Url = url;
                    temp.Add(podcasts);
                    numberOfPodcasts++;
                }
            }
            return temp;
        }
    }
}
