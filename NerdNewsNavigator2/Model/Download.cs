// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Model;

/// <summary>
/// A class for storing <see cref="Download"/> in a Database.
/// </summary>
[Table("Downloads")]
public partial class Download : Shared
{
    [ObservableProperty]
    private string _copyRight;
    [ObservableProperty]
    private string _fileName;
    [ObservableProperty]
    private bool _deleted;
}
