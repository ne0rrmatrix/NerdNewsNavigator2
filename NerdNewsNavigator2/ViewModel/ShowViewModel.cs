using CodeHollow.FeedReader.Feeds.Itunes;
using CodeHollow.FeedReader;
using System.Collections.ObjectModel;
using NerdNewsNavigator2.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using NerdNewsNavigator2.View;
using CommunityToolkit.Mvvm.Input;

namespace NerdNewsNavigator2.ViewModel;

[QueryProperty("param", "param")]
public partial class ShowViewModel : ObservableObject
{
    public ObservableCollection<Show> Shows { get; set; } = new();
    public string param
    {
        set
        {
            this.Shows = ShowViewModel.GetShow(value);
            OnPropertyChanged(nameof(Shows));
        }
    }
    public ShowViewModel()
    {

    }

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

    [RelayCommand]
    async Task Tap(string Url)
    {
        await Shell.Current.GoToAsync($"{nameof(PlayPodcastPage)}?Url={Url}");
    }
}
