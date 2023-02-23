// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Model;
/// <summary>
/// a Class for Storing <see cref="Position"/> in Database
/// </summary>
[Table("Positions")]
public class Position
{
    /// <summary>
    /// The <c>Id</c>  is a <see cref="int"/> and <see cref="PrimaryKeyAttribute"/> of <see cref="Podcast"/> Class.
    /// </summary>
    [PrimaryKey, AutoIncrement, Column("Id")]
    public int Id { get; set; }
    /// <summary>
    /// The <c>Title</c> is a <see cref="string"/> of <see cref="Position"/> Class.
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// The <c>SavedPosition</c> is a <see cref="TimeSpan"/> of <see cref="Position"/> Class.
    /// </summary>
    public TimeSpan SavedPosition { get; set; }
}
