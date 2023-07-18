// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Model;

/// <summary>
/// <c>Show</c> is a Class for storing Shows
/// </summary>
[Table("AllShows")]
public class Show : Shared
{
    public bool IsDownloading { get; set; } = false;
    public string Status { get; set; } = string.Empty;
    public string ImageFileLocation { get; set; } = string.Empty;
}
