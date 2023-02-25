// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;
/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="UpdateSettingsPage"/>
/// </summary>
public partial class UpdateSettingsViewModel : BaseViewModel
{
    readonly ILogger<UpdateSettingsViewModel> _logger;
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSettingsViewModel"/> class.
    /// </summary>
    public UpdateSettingsViewModel(ILogger<UpdateSettingsViewModel> logger)
        : base(logger)
    {
        Shell.Current.FlyoutIsPresented = false;
        IsBusy = true;
        OnPropertyChanged(nameof(IsBusy));
        ThreadPool.QueueUserWorkItem(DeleteAllPodcasts);
        ThreadPool.QueueUserWorkItem(ForceReset);
        _logger = logger;
    }
    private async void Next(Object stateInfo)
    {
        await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync($"{nameof(TabletPodcastPage)}"));
    }
    /// <summary>
    /// A Method to delete the <see cref="List{T}"/> of <see cref="Podcast"/>
    /// </summary>
    private async void DeleteAllPodcasts(Object stateInfo)
    {
        try
        {
            while (IsBusy)
            {
                await App.PositionData.DeleteAll();
                await App.PositionData.DeleteAllPodcasts();
                await App.PositionData.DeleteAllDownloads();
                IsBusy = false;
                ThreadPool.QueueUserWorkItem(Next);
            }
        }
        catch { }
    }
    /// <summary>
    /// Method force resets application database.
    /// </summary>
    /// <param name="stateinfo"></param>
    /// <returns></returns>
    public async void ForceReset(Object stateinfo)
    {
        Shows.Clear();
        Podcasts.Clear();
        await PodcastServices.AddDefaultPodcasts();
        foreach (var show in Podcasts.ToList())
        {
            var item = await FeedService.GetShows(show.Url, true);
            MostRecentShows.Add(item.First());
        }
    }
}
