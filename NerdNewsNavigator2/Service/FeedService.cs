// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;
/// <summary>
/// Class <c>FeedService</c> Get Podcasts from twit.tv
/// </summary>
public static class FeedService
{
    #region Get the Podcasts
    /// <summary>
    /// Method <c>GetFeed</c> return Feed from URL.
    /// </summary>
    /// <param name="item"></param> The URL of Podcast.
    /// <returns><see cref="Podcast"/></returns>
    public static Task<Podcast> GetFeed(string item)
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

        return Task.FromResult(feed);
    }
    #endregion

    #region Get the Shows
    /// <summary>
    /// Method <c>GetShow</c> returns list of Shows
    /// </summary>
    /// <param name="items"></param> The URL of the Show
    /// <returns><see cref="List{T}"/> <see cref="Show"/></returns>
    public static Task<List<Show>> GetShow(string items)
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
        return Task.FromResult(shows);
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
}
