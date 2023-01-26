﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

public partial class DesktopPodcastViewModel : ObservableObject, IPodcastPage
{
    #region Properties
    private readonly INavigation _navigation;

    public ObservableCollection<Podcast> Podcasts { get; set; } = new();
    #endregion

    public DesktopPodcastViewModel(INavigation navigation)
    {
        this._navigation = navigation;
        _ = GetPodcasts();
    }
    #region Get the Podcast and set the Podcast List
    async Task GetPodcasts()
    {
        var podcastList = await TwitService.GetListOfPodcasts();
        foreach (var item in podcastList)
        {
            var temp = await Task.FromResult(FeedService.GetFeed(item));
            Podcasts.Add(temp);
        }
    }
    #endregion

    [RelayCommand]
    async Task Tap(string url)
    {
      //  System.Diagnostics.Debug.WriteLine("String url: " + url);
        var encodedUrl = HttpUtility.UrlEncode(url);
      //  await _navigation.PushAsync(ViewService.ResolvePage<DesktopShowPage>());
       // await Shell.Current.GoToAsync($"{nameof(DesktopShowPage)}?Url={encodedUrl}");
        await _navigation.PushAsync(new DesktopShowPage(encodedUrl));
    }
}
