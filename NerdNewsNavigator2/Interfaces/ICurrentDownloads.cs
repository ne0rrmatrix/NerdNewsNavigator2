// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Interfaces;
public interface ICurrentDownloads
{
    public EventHandler<DownloadEventArgs> DownloadCancelled { get; set; }
    public EventHandler<DownloadEventArgs> DownloadFinished { get; set; }
    public EventHandler<DownloadEventArgs> DownloadStarted { get; set; }
#if ANDROID || IOS
    public void StartedDownload(string title, Show item, Plugin.LocalNotification.NotificationRequest notification);
#else
    public void StartedDownload(string title, Show item);
#endif
#if ANDROID || IOS
    public void Cancel(Show item, Plugin.LocalNotification.NotificationRequest notification);
#else
    public void Cancel(Show item);
#endif
#if ANDROID || IOS
    public void Completed(Show item, Plugin.LocalNotification.NotificationRequest notification);
#else
    public void Completed(Show item);
#endif

}
