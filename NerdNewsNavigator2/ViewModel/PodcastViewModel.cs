// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;
public partial class PodcastViewModel : BaseViewModel
{
    #region Properties
    readonly TwitService _twitService;
    public ObservableCollection<Podcast> Podcasts { get; set; } = new();
    #endregion
    public PodcastViewModel(TwitService twit)
    {
        this._twitService = twit;
        _ = GetPodcasts();
    }
    #region Get the Podcast and set the Podcast List
    async Task GetPodcasts()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var temp = await _twitService.GetPodcasts();

            foreach (var podcast in temp)
            {
                Podcasts.Add(podcast);
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
    async Task Tap(string url) => await Shell.Current.GoToAsync($"{nameof(ShowPage)}?Url={url}");
}
