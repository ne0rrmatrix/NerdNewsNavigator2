// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Primitives;
public class DownloadEventArgs : EventArgs
{
    public List<Show> Shows { get; set; }
    public Show Item { get; set; }
    public string Title { get; set; }
#if ANDROID || IOS
    public NotificationRequest Notification { get; set; }
#endif
}
