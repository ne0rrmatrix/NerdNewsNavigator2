// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;

public partial class DownloadShowsService : ObservableObject, IDownloadShows
{
    [ObservableProperty]
    private ObservableCollection<Download> _downloadedShows;
    public DownloadShowsService()
    {
        _downloadedShows = [];
        BindingBase.EnableCollectionSynchronization(DownloadedShows, null, ObservableCollectionCallback);
    }
    public void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
    {
        // `lock` ensures that only one thread access the collection at a time
        lock (collection)
        {
            accessMethod?.Invoke();
        }
    }
    /// <summary>
    /// A method that sets <see cref="DownloadedShows"/> from the database.
    /// </summary>
    public async Task<ObservableCollection<Download>> GetDownloadedShows()
    {
        DownloadedShows.Clear();
        var temp = await App.PositionData.GetAllDownloads();
        temp.Where(x => !x.Deleted).ToList().ForEach(DownloadedShows.Add);
        return DownloadedShows;
        //_logger.Info("Add all downloads to All Shows list");
    }
}
