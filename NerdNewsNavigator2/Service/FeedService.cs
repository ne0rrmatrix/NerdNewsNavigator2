// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;
public partial class FeedService
{
    public List<Podcast> _podcasts = new();
    public FeedService()
    {
        _podcasts.Clear();
    }
    #region Get the Podcasts
    public List<Podcast> GetData()
    {
        return _podcasts;
    }
    public static Podcast GetFeed(string item)
    {
        var counter = 0;
        Podcast feed = new();
        foreach (var level1Element in XElement.Load(item).Elements("channel"))
        {
            feed.Title = level1Element.Element("title").Value;
            feed.Description = level1Element.Element("description").Value;
            feed.Url = item;
            feed.Id = counter;
            counter++;

            foreach (var level2Element in level1Element.Elements("image"))
            {
                feed.Image = level2Element.Element("url")?.Value;
            }
        }
        return feed;
    }
    #endregion

    #region Get the Shows
    public static List<Show> GetShow(string items)
    {
        List<Show> shows = new();

        XmlDocument rssDoc = new();
        rssDoc.Load(items);
        var mgr = new XmlNamespaceManager(rssDoc.NameTable);
        mgr.AddNamespace("itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd");
        mgr.AddNamespace("media", "http://search.yahoo.com/mrss/");
        var rssNodes = rssDoc.SelectNodes("/rss/channel/item");

        if (rssNodes != null)
            foreach (XmlNode node in rssNodes)
            {
                Show show = new()
                {
                    Description = RemoveBADHtmlTags(node.SelectSingleNode("description") != null ? node.SelectSingleNode("description").InnerText : string.Empty),
                    Title = node.SelectSingleNode("title") != null ? node.SelectSingleNode("title").InnerText : string.Empty,
                    Url = node.SelectSingleNode("enclosure", mgr) != null ? node.SelectSingleNode("enclosure", mgr).Attributes["url"].InnerText : string.Empty,
                    Image = node.SelectSingleNode("itunes:image", mgr) != null ? node.SelectSingleNode("itunes:image", mgr).Attributes["href"].InnerText : string.Empty,
                };
                shows.Add(show);
            }
        return shows;
    }
    #endregion
    public static string RemoveBADHtmlTags(string hTMLCode)
    {
        hTMLCode = MyRegex().Replace(hTMLCode, "\"");
        return hTMLCode;
    }

    [GeneratedRegex("/\\?.*?.\"", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-CA")]
    private static partial Regex MyRegex();
}
