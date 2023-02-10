// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

public partial class SettingsViewModel : ObservableObject
{
    public ObservableCollection<Podcast> Podcasts { get; set; } = new();
    public PodcastServices _podcastServices { get; set; }
    public SettingsViewModel(PodcastServices podcastServices)
    {
        this._podcastServices = podcastServices;
        foreach (var item in _podcastServices.Current)
        {
            Podcasts.Add(item);
        }
        OnPropertyChanged(nameof(Podcast));
    }

    [RelayCommand]
    public async Task Tap(int id)
    {
        await _podcastServices.Delete(id);
        var temp = _podcastServices.Current;
        Podcasts.Clear();
        foreach (var item in temp)
        {
            Podcasts.Add(item);
        }
        OnPropertyChanged(nameof(Podcast));
    }
}
