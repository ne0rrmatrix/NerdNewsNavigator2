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
    public async Task Update(Show show)
    {
        var item = await SetShowAsync(show);
        System.Diagnostics.Debug.WriteLine($"Dnow received: {show.Title}");
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
    private static async Task<Show> SetShowAsync(Show show)
    {
        System.Diagnostics.Debug.WriteLine($"SetShow received {show.Title}");
        var downloadedShows = await App.PositionData.GetAllDownloads();
        if (downloadedShows.Find(x => x.Url == show.Url) is not null)
        {
            show.IsDownloaded = true;
            show.IsNotDownloaded = false;
            show.IsDownloading = false;
            return show;
        }
        if (App.CurrenDownloads.Find(x => x.Url == show.Url) is not null)
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
        }
        return show;
    }
}

