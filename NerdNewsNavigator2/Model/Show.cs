using CodeHollow.FeedReader.Feeds.Itunes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdNewsNavigator2.Model
{
    public class Show
    {
        public ItunesImage image { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public int id { get; set; }

        public Show(string Title, string Description, string URL, int Id, ItunesImage Image)
        {
            title = Title;
            description = Description;
            url = URL;
            id = Id;
            image = Image;
        }

    }
}
