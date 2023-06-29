// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Model;
public class Shared
{
    [PrimaryKey, AutoIncrement, Column("Id")]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Link { get; set; }
    public string Image { get; set; }
    public string Url { get; set; }
    public DateTime PubDate { get; set; }
    public bool Download { get; set; }
    public bool IsDownloaded { get; set; }
    public bool IsNotDownloaded { get; set; }
}
