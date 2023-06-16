// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Model;

/// <summary>
/// <c>Show</c> is a Class for storing Shows
/// </summary>
/// 
[Table("Show")]
public class Show : Shared
{
    /// <summary>
    /// the <c>IsDownloading</c> is a <see cref="bool"/> of <see cref="Show"/> Class.
    /// </summary>
    public bool IsDownloading { get; set; } = false;
    /// <summary>
    /// the <c>Status</c> is a <see cref="string"/> of <see cref="Show"/> Class.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
