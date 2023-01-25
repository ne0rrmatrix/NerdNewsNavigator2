// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

[QueryProperty("Url", "Url")]
public partial class ShowViewModel : ObservableObject
{
    #region Properties
    readonly TwitService _twitService;
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
    public ShowViewModel(TwitService twitService)
    {
        _twitService = twitService;
    }
    #region Get the Show and Set Show List
    async Task GetShows(string url)
    {
        try
        {
            var temp = await TwitService.GetShow(url);

            if (Shows.Count != 0)
                Shows.Clear();

            foreach (var show in temp)
            {
                Shows.Add(show);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"Unable to display Podcasts: {ex.Message}", "Ok");
        }
    }
    #endregion

    [RelayCommand]
    async Task Tap(string url) => await Shell.Current.GoToAsync($"{nameof(PlayPodcastPage)}?Url={url}");
}
