// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;

/// <summary>
/// A class that manages getting <see cref="Podcast"/> and <see cref="Show"/> from RSS feeds.
/// </summary>
public static class FeedService
{
    #region Get the Podcasts

    /// <summary>
    /// Method <c>GetFeed</c> return Feed from URL.
    /// </summary>
    /// <param name="item">The URL of <see cref="Podcast"/></param> 
    /// <returns><see cref="Podcast"/></returns>
    public static Task<Podcast> GetFeed(string item)
    {
        var counter = 0;
        Podcast feed = new();
        try
        {
            foreach (var level1Element in XElement.Load(item).Elements("channel"))
            {
                feed.Title = level1Element.Element("title").Value;
                feed.Description = level1Element.Element("description").Value;
                feed.Url = item;
                feed.Id = counter;
                feed.Download = false;
                counter++;

                foreach (var level2Element in level1Element.Elements("image"))
                {
                    feed.Image = level2Element.Element("url")?.Value;
                }
            }

            return Task.FromResult(feed);
        }
        catch
        {
            return Task.FromResult(feed);
        }
    }
    #endregion

    #region Get the Shows

    /// <summary>
    /// 
    /// </summary>
    /// <param name="items">The Url of the <see cref="Show"/></param>
    /// <param name="getFirstOnly"><see cref="bool"/> Get only first item.</param>
    /// <returns><see cref="List{T}"/> <see cref="Show"/></returns>
    public static Task<List<Show>> GetShows(string items, bool getFirstOnly)
    {
        List<Show> shows = new();
        XmlDocument rssDoc = new();
        try
        {
            rssDoc.Load(items);
            var mgr = new XmlNamespaceManager(rssDoc.NameTable);
            mgr.AddNamespace("itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd");
            mgr.AddNamespace("media", "http://search.yahoo.com/mrss/");
            var rssNodes = rssDoc.SelectNodes("/rss/channel/item");
            if (rssNodes == null)
                return Task.FromResult(shows);
            foreach (XmlNode node in rssNodes)
            {
                Show show = new()
                {
                    Description = RemoveBADHtmlTags(node.SelectSingleNode("description") != null ? node.SelectSingleNode("description").InnerText : string.Empty),
                    PubDate = ConvertToDateTime(node.SelectSingleNode("pubDate") != null ? node.SelectSingleNode("pubDate").InnerText : string.Empty),
                    Title = node.SelectSingleNode("title") != null ? node.SelectSingleNode("title").InnerText : string.Empty,
                    Url = node.SelectSingleNode("enclosure", mgr) != null ? node.SelectSingleNode("enclosure", mgr).Attributes["url"].InnerText : string.Empty,
                    Image = node.SelectSingleNode("itunes:image", mgr) != null ? node.SelectSingleNode("itunes:image", mgr).Attributes["href"].InnerText : string.Empty,
                };
                shows.Add(show);
                if (getFirstOnly)
                {
                    return Task.FromResult(shows);
                }
            }
            return Task.FromResult(shows);
        }
        catch
        {
            return Task.FromResult(shows);
        }
    }
    #endregion

    /// <summary>
    /// Method <c>hTMLCode</c> Returns html string that will not crash.
    /// </summary>
    /// <param name="hTMLCode"></param> String of html.
    /// <returns><see cref="string"/></returns>
    public static string RemoveBADHtmlTags(string hTMLCode)
    {
        hTMLCode = Regex.Replace(hTMLCode, "/\\?.*?.\"", "\"", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return hTMLCode;
    }

    /// <summary>
    /// Method returns <see cref="DateTime"/> object from string.
    /// </summary>
    /// <param name="dateTime"> DateTime <see cref="string"/></param>
    /// <returns><see cref="DateTime"/></returns>
    public static DateTime ConvertToDateTime(string dateTime)
    {
        return DateTime.Parse(dateTime.Remove(25));
    }
}
