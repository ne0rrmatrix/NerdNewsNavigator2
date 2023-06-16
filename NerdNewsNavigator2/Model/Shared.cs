// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Model;
interface IShared
{
    /// <summary>
    /// The <c>Id</c> is a <see cref="int"/> and  <see cref="PrimaryKeyAttribute"/> for <see cref="Download"/>
    /// </summary>
    [PrimaryKey, AutoIncrement, Column("Id")]
    public int Id { get; set; }
    /// <summary>
    /// The <c>Title</c> is a <see cref="string"/> of <see cref="Show"/> Class
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// The <c>Description</c> is a <see cref="string"/> of <see cref="Show"/> Class.
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// The <c>Link</c> is a <see cref="string"/> of <see cref="Show"/> Class.
    /// </summary>
    public string Link { get; set; }
    /// <summary>
    /// The <c>Image</c> is a <see cref="string"/> of <see cref="Show"/> Class.
    /// </summary>
    public string Image { get; set; }
    /// <summary>
    /// The <c>Url</c> is a <see cref="string"/> of <see cref="Show"/> Class.
    /// </summary>
    public string Url { get; set; }
    /// <summary>
    /// the <c>PubDate</c> is a <see cref="DateTime"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public DateTime PubDate { get; set; }
    /// <summary>
    /// the <c>Download</c> is a <see cref="bool"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public bool Download { get; set; }
    /// <summary>
    /// the <c>IsDownloaded</c> is a <see cref="bool"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public bool IsDownloaded { get; set; }

    /// <summary>
    /// the <c>IsNotDownloaded</c> is a <see cref="bool"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public bool IsNotDownloaded
    {
        get => !Download;
    }
}
public class Shared
{
    /// <summary>
    /// The <c>Id</c> is a <see cref="int"/> and  <see cref="PrimaryKeyAttribute"/> for <see cref="Podcast"/>
    /// </summary>
    [PrimaryKey, AutoIncrement, Column("Id")]
    public int Id { get; set; }
    /// <summary>
    /// The <c>Title</c> is a <see cref="string"/> of <see cref="Show"/> Class
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// The <c>Description</c> is a <see cref="string"/> of <see cref="Show"/> Class.
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// The <c>Link</c> is a <see cref="string"/> of <see cref="Show"/> Class.
    /// </summary>
    public string Link { get; set; }
    /// <summary>
    /// The <c>Image</c> is a <see cref="string"/> of <see cref="Show"/> Class.
    /// </summary>
    public string Image { get; set; }
    /// <summary>
    /// The <c>Url</c> is a <see cref="string"/> of <see cref="Show"/> Class.
    /// </summary>
    public string Url { get; set; }
    /// <summary>
    /// the <c>PubDate</c> is a <see cref="DateTime"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public DateTime PubDate { get; set; }
    /// <summary>
    /// the <c>Download</c> is a <see cref="bool"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public bool Download { get; set; }
    /// <summary>
    /// the <c>IsDownloaded</c> is a <see cref="bool"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public bool IsDownloaded { get; set; }

    /// <summary>
    /// the <c>IsNotDownloaded</c> is a <see cref="bool"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public bool IsNotDownloaded { get; set; }
}

