// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;
public partial class CurrentDownloads : ObservableObject, ICurrentDownloads
{
    public EventHandler<DownloadEventArgs> DownloadCancelled { get; set; }
    public EventHandler<DownloadEventArgs> DownloadFinished { get; set; }
    public EventHandler<DownloadEventArgs> DownloadStarted { get; set; }
    public List<Show> Shows { get; set; }
    public CurrentDownloads()
    {
        Shows = [];
    }
    #region EventArgs
#if ANDROID || IOS
    public void StartedDownload(string title, Show item, Plugin.LocalNotification.NotificationRequest notification)
#else
    public void StartedDownload(string title, Show item)
#endif
    {
        var args = new DownloadEventArgs
        {
            Title = title,
            Item = item,
            Shows = Shows,
#if ANDROID || IOS
            Notification = notification
#endif
        };
        OnStarted(args);
    }
#if ANDROID || IOS
    public void Cancel(Show item, Plugin.LocalNotification.NotificationRequest notification)
#else
    public void Cancel(Show item)
#endif
    {
        var args = new DownloadEventArgs
        {
            Item = item,
            Title = string.Empty,
            Shows = Shows,
#if ANDROID || IOS
            Notification = notification
#endif
        };
        OnCancelled(args);
    }
#if ANDROID || IOS
    public void Completed(Show item, Plugin.LocalNotification.NotificationRequest notification)
#else
    public void Completed(Show item)
#endif
    {
        var args = new DownloadEventArgs
        {
            Item = item,
            Title = string.Empty,
            Shows = Shows,
#if ANDROID || IOS
            Notification = notification
#endif
        };
        OnDownloadFinished(args);
    }
    protected virtual void OnCancelled(DownloadEventArgs args)
    {
        DownloadCancelled?.Invoke(this, args);
    }
    protected virtual void OnStarted(DownloadEventArgs args)
    {
        DownloadStarted?.Invoke(this, args);
    }
    protected virtual void OnDownloadFinished(DownloadEventArgs e)
    {
        DownloadFinished?.Invoke(this, e);
    }
    #endregion
}
