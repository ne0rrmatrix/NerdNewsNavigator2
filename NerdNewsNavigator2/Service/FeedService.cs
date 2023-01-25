// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;
public class FeedService
{
    public List<Podcast> _podcasts = new();
    #region Get the Podcasts
    public static FeedService GetFeed(string item)
    {
        int counter = 0;
        FeedService feed = new();
        try
        {
            foreach (var level1Element in XElement.Load(item).Elements("channel"))
            {
                var podcast = new Podcast
                {
                    Title = level1Element.Element("title").Value,
                    Description = level1Element.Element("description").Value,
                    Url = item
                };
                foreach (var level2Element in level1Element.Elements("image"))
                {
                    podcast.Image = level2Element.Element("url")?.Value;
                }
                counter += 1;
                feed._podcasts.Add(podcast);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex.InnerException);
        }
        return feed;
    }
    #endregion
    #region Get the Shows
    public static FeedService GetShow(string items)
    {
        int counter = 0;
        FeedService feed = new();
        try
        {
            #region Get the Feed from the Internet
            Podcast podcast = new();

            XmlDocument rssDoc = new();
            rssDoc.Load(items);
            XmlNamespaceManager mgr = new XmlNamespaceManager(rssDoc.NameTable);
            mgr.AddNamespace("itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd");
            mgr.AddNamespace("media", "http://search.yahoo.com/mrss/");
            var rssNodes = rssDoc.SelectNodes("/rss/channel/item");
            counter += 1;

            if (rssNodes != null)
                foreach (XmlNode node in rssNodes)
                {
                    Show show = new()
                    {
                        Description = node.SelectSingleNode("description") != null ? node.SelectSingleNode("description").InnerText : string.Empty,
                        Title = node.SelectSingleNode("title") != null ? node.SelectSingleNode("title").InnerText : string.Empty,
                        Url = node.SelectSingleNode("enclosure", mgr) != null ? node.SelectSingleNode("enclosure", mgr).Attributes["url"].InnerText : string.Empty,
                        Image = node.SelectSingleNode("itunes:image", mgr) != null ? node.SelectSingleNode("itunes:image", mgr).Attributes["href"].InnerText : string.Empty,
                    };
                    podcast.Add(show);
                }
            feed.AddPodcast(podcast);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex.InnerException);
        }
        #endregion
        return feed;
    }
    #endregion
    public void AddPodcast(Podcast item)
    {
        _podcasts.Add(item);
    }
}
