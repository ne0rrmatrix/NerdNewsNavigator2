// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

[QueryProperty("Url", "Url")]
public partial class DesktopShowViewModel : ObservableObject, IShowPage
{
    #region Properties
    private readonly INavigation _navigation;
    public ObservableCollection<Show> Shows { get; set; } = new();
    public string Url
    {
        set
        {
            var decodedUrl = HttpUtility.UrlDecode(value);
            _ = GetShows(decodedUrl);
            OnPropertyChanged(nameof(Shows));
        }
    }
    #endregion
   public DesktopShowViewModel(string url)
    {
        this.Url = HttpUtility.UrlDecode(url);
    }
    public DesktopShowViewModel(INavigation navigation)
    {
        this._navigation = navigation;
    }
    #region Get the Show and Set Show List
    async Task GetShows(string url)
    {
        if (Shows.Count != 0)
            Shows.Clear();

        var temp = await TwitService.GetShow(url);
        Shows = new ObservableCollection<Show>(temp);
    }
    #endregion

    [RelayCommand]
    async Task Tap(string url) { System.Diagnostics.Debug.WriteLine("String is: " + url); await _navigation.PushAsync(new DesktopPlayPodcastPage(url)); }
    //async Task Tap(string url) => await Shell.Current.GoToAsync($"{nameof(DesktopPlayPodcastPage)}?Url={url}");
}
