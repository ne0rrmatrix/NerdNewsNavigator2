// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that manages watching Live video from twit.tv podcasting network
/// </summary>
public partial class LivePage : ContentPage
{
    private YoutubeClient Youtube { get; set; } = new();
    private HttpClient Client { get; set; } = new();
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
        var item = "https://www.youtube.com/user/twit";
        _ = LoadVideo(item);
    }

    /// <summary>
    /// Method Starts <see cref="MediaElement"/> Playback.
    /// </summary>
    /// <returns></returns>
    private async Task LoadVideo(string url)
    {
        mediaElement.IsYoutube = true;
        var m3u = await ParseVideoIdAsync(url);
        if (m3u != string.Empty)
        {
            mediaElement.Source = ParseM3UPLaylist(await GetM3U_Url(m3u));
            mediaElement.Play();
        }
    }

    /// <summary>
    /// Method returns Video ID from Youtube <see cref="string"/>, by way of Username/live.
    /// </summary>
    ///  <param name="url"></param>
    /// <returns></returns>
    private async Task<string> ParseVideoIdAsync(string url)
    {
        var userId = (await Youtube.Channels.GetByUserAsync(url)).Id;
        var page = await Client.GetAsync("https://www.youtube.com/channel/" + $"{userId}/live");
        var result = await page.Content.ReadAsStringAsync();
        if (result is null)
        {
            return string.Empty;
        }
        return result.Substring(result.IndexOf("watch?v=") + 8, 11);
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
        Add(list);
        return list[list.FindIndex(x => x.Resolution.Height == 720)].Uri;
    }

    /// <summary>
    /// Method creates <see cref="List{T}"/> of URL's from <see cref="M3U8Parser.ExtXType.StreamInf"/>
    /// </summary>
    /// <param name="item"></param>
    private void Add(List<M3U8Parser.ExtXType.StreamInf> item)
    {
        item?.ForEach(x =>
        {
            var temp = new YoutubeResolutions
            {
                Title = $"{x.Resolution.Height}P",
                Url = x.Uri.ToString()
            };
            Items.Add(temp);
        });
    }

    /// <summary>
    /// Method returns the Live stream M3U Url from Youtube ID.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private async Task<string> GetM3U_Url(string url)
    {
        var content = string.Empty;
        var result = await Youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(url);
        var response = await Client.GetAsync(result);
        if (response.IsSuccessStatusCode)
        {
            content = await response.Content.ReadAsStringAsync();
        }
        return content;
    }
}
