// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Model;
[Table("Podcasts")]
public class Podcast
{
    [PrimaryKey, AutoIncrement, Column("Id")]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Link { get; set; }
    public string PubDate { get; set; }
    public string EnclosureUrl { get; set; }
    public string EnclosureType { get; set; }
    public string CopyRight { get; set; }
    public string Image { get; set; }
    public string Url { get; set; }
    public List<Show> _shows = new();
    public void Add(Show item)
    {
        _shows.Add(item);
    }
    public List<Show> GetShows()
    {
        return _shows;
    }
}
