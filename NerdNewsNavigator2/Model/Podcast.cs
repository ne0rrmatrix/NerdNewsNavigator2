// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Model;

/// <summary>
/// A class for storing <see cref="Podcast"/> in a Database.
/// </summary>
[Table("Podcasts")]
public partial class Podcast : SharedModels
{
    [ObservableProperty]
    private bool _deleted;
}
