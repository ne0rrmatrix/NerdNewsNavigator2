// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Interfaces;

public interface IPodcastService
{
    public ObservableCollection<Podcast> Podcasts { get; set; }
    public List<Podcast> GetFromUrl();
    public Task AddToDatabase(List<Podcast> podcast);
    public Task AddPodcast(string url);
    public void AddDefaultPodcasts();
    public Task RemoveDefaultPodcasts();
    public void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess);
    public Task<ObservableCollection<Podcast>> GetPodcasts();
}
