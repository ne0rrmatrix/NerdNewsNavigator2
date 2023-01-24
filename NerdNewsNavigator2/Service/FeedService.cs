// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Xml;

namespace NerdNewsNavigator2.Service;
public class FeedService
{

    public List<Podcast> Podcasts = new();
    public FeedService GetPodcast(string item)
    {
        int counter = 0;
        FeedService feed = new();
        try
        {
            #region Get the Feed from the Internet
            Podcast podcast = new();

            XmlDocument rssDoc = new();
            rssDoc.Load(item);
            XmlNamespaceManager mgr = new XmlNamespaceManager(rssDoc.NameTable);
            mgr.AddNamespace("itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd");
            XmlNodeList rssNodes = rssDoc.SelectNodes("/rss/channel/item");
            podcast.Title = rssDoc.SelectSingleNode("/rss/channel/title") != null ? rssDoc.SelectSingleNode("/rss/channel/title").InnerText : string.Empty;
            podcast.Description = rssDoc.SelectSingleNode("/rss/channel/description") != null ? rssDoc.SelectSingleNode("/rss/channel/description").InnerText : string.Empty;
            podcast.Image = rssDoc.SelectSingleNode("/rss/channel/image/url") != null ? rssDoc.SelectSingleNode("/rss/channel/image/url").InnerText : string.Empty;
            podcast.Url = item;

            counter += 1;
            feed.AddPodcast(podcast);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex.InnerException);
        }
        #endregion
        return feed;
    }
    public FeedService GetShow(string item)
    {
        int counter = 0;
        FeedService feed = new();
        try
        {
            #region Get the Feed from the Internet
            Podcast podcast = new();

            XmlDocument rssDoc = new();
            rssDoc.Load(item);
            XmlNamespaceManager mgr = new XmlNamespaceManager(rssDoc.NameTable);
            mgr.AddNamespace("itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd");
            XmlNodeList rssNodes = rssDoc.SelectNodes("/rss/channel/item");
            podcast.Url = item;
            counter += 1;

            if (rssNodes != null)
                foreach (XmlNode node in rssNodes)
                {
                    Show show = new()
                    {
                        Description = node.SelectSingleNode("description") != null ? node.SelectSingleNode("description").InnerText : string.Empty,
                        Title = node.SelectSingleNode("title") != null ? node.SelectSingleNode("title").InnerText : string.Empty,
                        Url = node.SelectSingleNode("enclosure") != null ? node.SelectSingleNode("enclosure").Attributes["url"].InnerText : string.Empty,
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
    public FeedService GetFeed(string item)
    {
        int counter = 0;
        FeedService feed = new();
        try
        {
            #region Get the Feed from the Internet
            Podcast podcast = new();

            XmlDocument rssDoc = new();
            rssDoc.Load(item);
            XmlNamespaceManager mgr = new XmlNamespaceManager(rssDoc.NameTable);
            mgr.AddNamespace("itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd");
            XmlNodeList rssNodes = rssDoc.SelectNodes("/rss/channel/item");
            podcast.Link = rssDoc.SelectSingleNode("/rss/channel/link") != null ? rssDoc.SelectSingleNode("/rss/channel/link").InnerText : string.Empty;
            podcast.Title = rssDoc.SelectSingleNode("/rss/channel/title") != null ? rssDoc.SelectSingleNode("/rss/channel/title").InnerText : string.Empty;
            podcast.Description = rssDoc.SelectSingleNode("/rss/channel/description") != null ? rssDoc.SelectSingleNode("/rss/channel/description").InnerText : string.Empty;
            podcast.Image = rssDoc.SelectSingleNode("/rss/channel/image/url") != null ? rssDoc.SelectSingleNode("/rss/channel/image/url").InnerText : string.Empty;
            podcast.Url = item;

            counter += 1;

            if (rssNodes != null)
                foreach (XmlNode node in rssNodes)
                {
                    Show show = new()
                    {
                        CopyRight = node.SelectSingleNode("copyright") != null ? node.SelectSingleNode("copyright").InnerText : string.Empty,
                        Description = node.SelectSingleNode("description") != null ? node.SelectSingleNode("description").InnerText : string.Empty,
                        Link = node.SelectSingleNode("link") != null ? node.SelectSingleNode("link").InnerText : string.Empty,
                        PubDate = node.SelectSingleNode("pubdate") != null ? node.SelectSingleNode("pubdate").InnerText : string.Empty,
                        Title = node.SelectSingleNode("title") != null ? node.SelectSingleNode("title").InnerText : string.Empty,
                        Url = node.SelectSingleNode("enclosure") != null ? node.SelectSingleNode("enclosure").Attributes["url"].InnerText : string.Empty,

                        EnclosureType = node.SelectSingleNode("enclosure") != null ? node.SelectSingleNode("enclosure").Attributes["type"].InnerText : string.Empty,

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
    public void AddPodcast(Podcast item)
    {
        Podcasts.Add(item);
    }
}
