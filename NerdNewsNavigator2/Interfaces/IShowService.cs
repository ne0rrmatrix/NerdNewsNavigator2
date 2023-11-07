// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Interfaces;

public interface IShowService
{
    public ObservableCollection<Show> Shows { get; set; }
    public ObservableCollection<Show> GetShowsAsync(string url, bool getFirstOnly);
    public void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess);
    public Task Play(string url);
    public void SetProperties(Show show);
}
