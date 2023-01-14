using CodeHollow.FeedReader.Feeds.Itunes;
using CodeHollow.FeedReader;
using System.Collections.ObjectModel;
using NerdNewsNavigator2.Model;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NerdNewsNavigator2.ViewModel;

[QueryProperty("param", "param")]
public partial class ShowViewModel : ObservableObject
{
    public ObservableCollection<Show> Shows { get; set; } = new();
    public string param
    {
        set
        {
            this.Shows = GetShow(value);
            OnPropertyChanged(nameof(Shows));

        }
    }
    public ShowViewModel()
    {

    }

    private ObservableCollection<Show> GetShow(string url)
    {
        ObservableCollection<Show> result = new();
      

        try
        {
            var feed = FeedReader.ReadAsync(url);
            foreach (var item in feed.Result.Items)
            {
                Show show = new();
                show.Title = item.Title;
                show.Description = item.Description;
                show.Image = item.GetItunesItem().Image.Href;
                show.Url = item.Id;
                result.Add(show);
            }
            return result;
        }
        catch
        {
            Show show = new();
            show.Title = string.Empty;
            result.Add(show);
            return result;
        }
    }
}
