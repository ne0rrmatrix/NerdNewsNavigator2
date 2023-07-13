// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2;

public class DownloadEventArgs : EventArgs
{
    public Show Item { get; set; }
}

public class DownloaddCompleted
{
    public event EventHandler<DownloadEventArgs> DownloadFinished;

    /// <summary>
    /// Update Download Status of Current Download
    /// </summary>
    /// <param name="url"></param>
    /// <param name="isDownloading"></param> Is <see cref="Show"/> downloading
    /// <param name="isDownloaded"></param> Is <see cref="Show"/> downloaded
    public void Update(string url, bool isDownloading, bool isDownloaded)
    {
        DownloadEventArgs args = new();
        args.Item.Url = url;
        args.Item.IsDownloading = isDownloading;
        args.Item.IsDownloaded = isDownloaded;
        OnDownloadFinished(args);
    }

    protected virtual void OnDownloadFinished(DownloadEventArgs e)
    {
        var handler = DownloadFinished;
        if (handler is not null)
        {
            handler(this, e);
        }
    }

}
