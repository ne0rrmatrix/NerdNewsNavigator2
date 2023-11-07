// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Interfaces;

public interface IDownloadShows
{
    public ObservableCollection<Download> DownloadedShows { get; set; }
    public void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess);
    public Task<ObservableCollection<Download>> GetDownloadedShows();
}
