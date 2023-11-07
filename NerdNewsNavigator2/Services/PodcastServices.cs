// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;

/// <summary>
/// A Class for managing Podcasts.
/// </summary>
public partial class PodcastServices : ObservableObject, IPodcastService
{
    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of <see cref="Podcast"/> managed by this class.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Podcast> _podcasts;

    private readonly IFeedService _iFeedService;
    public PodcastServices(IFeedService iFeedService)
    {
        _iFeedService = iFeedService;
        _podcasts = [];

        BindingBase.EnableCollectionSynchronization(Podcasts, null, ObservableCollectionCallback);
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
    /// <c>GetPodcasts</c> is a <see cref="Task"/> that sets <see cref="Podcasts"/> from either a Database or from the web.
    /// </summary>
    /// <returns></returns>
    public async Task<ObservableCollection<Podcast>> GetPodcasts()
    {
        if (Podcasts.Count > 0)
        {
            return new ObservableCollection<Podcast>(Podcasts);
        }
        var temp = await App.PositionData.GetAllPodcasts();
        if (temp.Count > 0)
        {
            Podcasts = new ObservableCollection<Podcast>(temp);
            return new ObservableCollection<Podcast>(temp);
        }
        var updates = GetFromUrl();
        var sortedItems = updates.OrderBy(x => x.Title).ToList();
        Podcasts = new ObservableCollection<Podcast>(sortedItems);
        await AddToDatabase(sortedItems);
        return new ObservableCollection<Podcast>(sortedItems);
    }
    public async Task UpdatePodcasts()
    {
        var temp = await App.PositionData.GetAllPodcasts();
        Podcasts = new ObservableCollection<Podcast>(temp);
    }
    /// <summary>
    /// Method Retrieves <see cref="List{T}"/> <see cref="Podcast"/> from default RSS Feeds.
    /// </summary>
    /// <returns><see cref="List{T}"/> <see cref="Podcast"/></returns>
    public List<Podcast> GetFromUrl()
    {
        List<Podcast> podcasts = [];
        var item = _iFeedService.GetPodcastListAsync();
        item.ForEach(x => podcasts.Add(_iFeedService.GetFeed(x)));
        return podcasts;
    }

    #region Manipulate Database

    /// <summary>
    /// Method Adds Playback <see cref="Podcast"/> to Database.
    /// </summary>
    /// <param name="podcast"></param> Position Class object.
    /// <returns>nothing</returns>
    public async Task AddToDatabase(List<Podcast> podcast)
    {
        for (var i = 0; i < podcast.Count; i++)
        {
            Position pos = new()
            {
                Title = podcast[i].Title,
                Description = podcast[i].Description,
                Url = podcast[i].Url,
                Image = podcast[i].Image,
                Download = podcast[i].Download,
                Link = podcast[i].Link,
                PubDate = podcast[i].PubDate
            };
            await App.PositionData.AddPosition(pos);
            await App.PositionData.AddPodcast(podcast[i]);
        }
    }

    /// <summary>
    /// Method Adds a <see cref="Podcast"/> to Database.
    /// </summary>
    /// <param name="url"><see cref="string"/> Url of <see cref="Podcast"/></param>
    /// <returns>nothing</returns>
    public async Task AddPodcast(string url)
    {
        var podcast = _iFeedService.GetFeed(url);
        await App.PositionData.AddPodcast(new Podcast
        {
            Title = podcast.Title,
            Url = podcast.Url,
            Download = false,
            IsNotDownloaded = true,
            Description = podcast.Description,
            Image = podcast.Image,
        });
    }

    /// <summary>
    /// Method Adds default <see cref="List{T}"/> <see cref="Podcast"/> from RSS feed to Database.
    /// </summary>
    /// <returns>nothing</returns>
    public void AddDefaultPodcasts()
    {
        ThreadPool.QueueUserWorkItem(async state =>
        {
            await RemoveDefaultPodcasts();
            var items = GetFromUrl();
            var res = items.OrderBy(x => x.Title).ToList();
            foreach (var item in res)
            {
                await App.PositionData.AddPodcast(item);
            }
        });
    }
    /// <summary>
    /// Method resets <see cref="List{T}"/> <see cref="Podcast"/> to default list.
    /// </summary>
    /// <returns>nothing</returns>
    public async Task RemoveDefaultPodcasts()
    {
        await Task.Run(async () =>
        {
            var items = await App.PositionData.GetAllPodcasts();
            items.Where(x => x.Url.Contains("feeds.twit.tv")).ToList().ForEach(async item => await App.PositionData.DeletePodcast(item));
        });

    }
    #endregion
}
