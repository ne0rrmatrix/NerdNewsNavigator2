// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that manages watching Live video from twit.tv podcasting network
/// </summary>
public partial class LivePage : ContentPage
{
    public ObservableCollection<YoutubeResolutions> Items { get; set; } = new();
    /// <summary>
    /// Initializes a new instance of <see cref="LivePage"/> class.
    /// </summary>
    /// <param name="liveViewModel">This classes <see cref="ViewModel"/> from <see cref="LiveViewModel"/></param>
    public LivePage(LiveViewModel liveViewModel)
    {
        InitializeComponent();
        BindingContext = liveViewModel;
    }

    /// <summary>
    /// Method overrides <see cref="OnDisappearing"/> to stop playback when leaving a page.
    /// </summary>
    protected override void OnDisappearing()
    {
        mediaElement.ShouldKeepScreenOn = false;
        mediaElement.Stop();
    }

    /// <summary>
    /// Method Loads Video after page has finished being rendered.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        _ = LoadVideo();
    }

    /// <summary>
    /// Method Starts <see cref="MediaElement"/> Playback.
    /// </summary>
    /// <returns></returns>
    private async Task LoadVideo()
    {
        mediaElement.IsYoutube = true;
        var m3u = await GetM3U_Url("F2NreNEmMy4");
        mediaElement.Source = ParseM3UPLaylist(m3u);
        mediaElement.Play();
    }

    /// <summary>
    /// Method returns 720P URL for <see cref="mediaElement"/> to Play.
    /// </summary>
    /// <param name="m3UString"></param>
    /// <returns></returns>
    private string ParseM3UPLaylist(string m3UString)
    {
        var masterPlaylist = MasterPlaylist.LoadFromText(m3UString);
        var list = masterPlaylist.Streams.ToList();
        foreach (var item in list)
        {
            Add(item);
        }
        return list.ElementAt(list.FindIndex(x => x.Resolution.Height == 720)).Uri;
    }
    private void Add(M3U8Parser.ExtXType.StreamInf item)
    {
        var temp = new YoutubeResolutions
        {
            Title = $"{item.Resolution.Height}P",
            Url = item.Uri.ToString()
        };
        Items.Add(temp);
    }
    /// <summary>
    /// Method returns the Live stream M3U Url from youtube ID.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private static async Task<string> GetM3U_Url(string url)
    {
        var content = string.Empty;
        var client = new HttpClient();
        var youtube = new YoutubeClient();
        var result = await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(url);
        var response = await client.GetAsync(result);
        if (response.IsSuccessStatusCode)
        {
            content = await response.Content.ReadAsStringAsync();
        }
        return content;
    }
}
