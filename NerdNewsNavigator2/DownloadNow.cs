// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2;

public class DownloadNow
{
    public delegate void DownloadCompletedEventHandler(object sender, DownloadEventArgs e);
    public event DownloadCompletedEventHandler DownloadCompleted;
    public DownloadNow()
    {
    }
    public void Update(Show show)
    {
        var item = SetShow(show);
        DownloadEventArgs args = new()
        {
            Item = item
        };
        OnDownloadCompleted(args);
    }
    protected virtual void OnDownloadCompleted(DownloadEventArgs e)
    {
        var handler = DownloadCompleted;
        if (handler is not null)
        {
            handler(this, e);
        }
    }
    private static Show SetShow(Show show)
    {
        if (show.IsDownloading)
        {
            show.IsDownloading = true;
            show.IsDownloaded = true;
            show.IsNotDownloaded = false;
            return show;
        }
        else
        {
            show.IsDownloading = false;
            show.IsDownloaded = false;
            show.IsNotDownloaded = true;
            return show;
        }
    }
}

