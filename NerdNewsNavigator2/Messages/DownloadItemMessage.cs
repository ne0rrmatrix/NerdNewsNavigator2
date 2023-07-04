// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Messages;
/// <summary>
/// A class that manages messages for downloading items.
/// </summary>
public class DownloadItemMessage : ValueChangedMessage<bool>
{
    public DownloadItemMessage(bool value, string title) : base(value)
    {
        Title = title;
    }
    public string Title { get; }
}
