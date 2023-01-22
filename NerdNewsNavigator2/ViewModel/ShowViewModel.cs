// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
namespace NerdNewsNavigator2.ViewModel;

[QueryProperty("Url", "Url")]
public partial class ShowViewModel : BaseViewModel
{
    #region Properties
    readonly TwitService _twitService;
    public ObservableCollection<Show> Shows { get; set; } = new();
    #endregion
    public ShowViewModel(TwitService twitService)
    {
        _twitService = twitService;
    }
    public string Url
    {
        set
        {
            _ = GetShows(value);
            OnPropertyChanged(nameof(Shows));
        }
    }
    public ShowViewModel()
    {

    }

    #region Get the Show
    async Task GetShows(string url)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var temp = await _twitService.GetShow(url);

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
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    [RelayCommand]
    async Task Tap(string url) => await Shell.Current.GoToAsync($"{nameof(PlayPodcastPage)}?Url={url}");
}
