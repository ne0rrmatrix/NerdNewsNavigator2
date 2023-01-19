// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using CodeHollow.FeedReader;
using CodeHollow.FeedReader.Feeds.Itunes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NerdNewsNavigator2.Model;
using NerdNewsNavigator2.View;

namespace NerdNewsNavigator2.ViewModel;

[QueryProperty("Url", "Url")]
public partial class ShowViewModel : ObservableObject
{
    #region Properties
    public ObservableCollection<Show> Shows { get; set; } = new();
    #endregion

    public string Url
    {
        set
        {
            this.Shows = ShowViewModel.GetShow(value);
            OnPropertyChanged(nameof(Shows));
        }
    }
    #region GetShow
    private static ObservableCollection<Show> GetShow(string url)
    {
        ObservableCollection<Show> result = new();
        try
        {
            var feed = FeedReader.ReadAsync(url);
            foreach (var item in feed.Result.Items)
            {
                Show show = new()
                {
                    Title = item.Title,
                    Description = item.Description,
                    Image = item.GetItunesItem().Image.Href,
                    Url = item.Id
                };
                result.Add(show);
            }
            return result;
        }
        catch
        {
            Show show = new()
            {
                Title = string.Empty,
            };
            result.Add(show);
            return result;
        }
    }
    #endregion

    [RelayCommand]
    async Task Tap(string Url) => await Shell.Current.GoToAsync($"{nameof(PlayPodcastPage)}?Url={Url}");
}
