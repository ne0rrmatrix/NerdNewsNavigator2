// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Messages;

public class NotificationItemMessage : ValueChangedMessage<int>
{
    public bool Cancel { get; }
    public int Id { get; }
    public string Url { get; }
    public Show ShowItem { get; }
    public NotificationItemMessage(int id, string url, Show show, bool cancel) : base(id)
    {
        Id = id;
        Url = url;
        ShowItem = show;
        Cancel = cancel;
    }
}
