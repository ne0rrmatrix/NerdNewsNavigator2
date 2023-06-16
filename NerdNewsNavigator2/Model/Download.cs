// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Model;

/// <summary>
/// A class for storing <see cref="Download"/> in a Database.
/// </summary>
[Table("Downloads")]
public class Download : Shared
{
    /// <summary>
    /// The <c>CopyRight</c> is a <see cref="string"/> of <see cref="Download"/> Class.
    /// </summary>
    public string CopyRight { get; set; }
    /// <summary>
    /// the <c>FileName</c> is a <see cref="string"/> of <see cref="Download"/> Class.
    /// </summary>
    public string FileName { get; set; }
    public bool Deleted { get; set; }
}
