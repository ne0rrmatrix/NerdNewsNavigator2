// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Primitives;
public class DownloadEventArgs : EventArgs
{
    public Show Item { get; set; }
    public bool Cancelled { get; set; }
    public string Status { get; set; }
#if ANDROID || IOS
    public NotificationRequest Notification { get; set; }
#endif
    public double Progress { get; set; }
}
