// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View.Mobile;
public partial class MobileShowPage : ContentPage, IShowPage
{
    private readonly INavigation _navigation;
    public ObservableCollection<Show> Shows { get; set; } = new();
    public MobileShowPage(INavigation navigation)
    {
        InitializeComponent();
        _navigation = navigation;
    }

    async Task GetShows(string url)
    {
        if (Shows.Count != 0)
            Shows.Clear();

        var temp = await TwitService.GetShow(url);
        Shows = new ObservableCollection<Show>(temp);
    }
    private void SwipedGesture(object sender, SwipedEventArgs e)
    {
        switch (e.Direction)
        {
            case SwipeDirection.Right:
                Shell.Current.GoToAsync("..");
                break;
        }
    }
    private void LivePage(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"{nameof(MobileLivePage)}");
    }
    private void PodcastPage(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"{nameof(MobilePodcastPage)}");
    }
}
