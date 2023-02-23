// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Model;
/// <summary>
/// A class for storing <see cref="Podcast"/> in a Database.
/// </summary>
[Table("Podcasts")]
public class Podcast
{
    /// <summary>
    /// The <c>Id</c> is a <see cref="int"/> and  <see cref="PrimaryKeyAttribute"/> for <see cref="Podcast"/>
    /// </summary>
    [PrimaryKey, AutoIncrement, Column("Id")]
    public int Id { get; set; }
    /// <summary>
    /// The <c>Title</c> is a <see cref="string"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// The <c>Description</c> is a <see cref="string"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// The <c>Link</c> is a <see cref="string"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public string Link { get; set; }
    /// <summary>
    /// The <c>PubDate</c> is a <see cref="string"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public string PubDate { get; set; }
    /// <summary>
    /// The <c>EnclosureUrl</c> is a <see cref="string"/> of <see cref="Podcast"/> Class
    /// </summary>
    public string EnclosureUrl { get; set; }
    /// <summary>
    /// The <c>EnclosureType</c> is a <see cref="Type"/> of <see cref="string"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public string EnclosureType { get; set; }
    /// <summary>
    /// The <c>CopyRight</c> is a <see cref="string"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public string CopyRight { get; set; }
    /// <summary>
    /// The <c>Image</c> is a <see cref="string"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public string Image { get; set; }
    /// <summary>
    /// the <c>Url</c> is a <see cref="string"/> of <see cref="Podcast"/> Class.
    /// </summary>
    public string Url { get; set; }
}
