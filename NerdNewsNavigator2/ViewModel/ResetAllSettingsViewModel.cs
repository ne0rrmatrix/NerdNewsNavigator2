// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;
/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="ResetAllSettingsViewModel"/>
/// </summary>
public partial class ResetAllSettingsViewModel : SharedViewModel
{
    private readonly IMessenger _messenger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetAllSettingsViewModel"/> class.
    /// </summary>
    public ResetAllSettingsViewModel(ILogger<ResetAllSettingsViewModel> logger, IConnectivity connectivity, IMessenger messenger) : base(logger, connectivity)
    {
        Shell.Current.FlyoutIsPresented = false;
        _messenger = messenger;
        ThreadPool.QueueUserWorkItem(async state => await ResetAll());
    }

    #region Methods
    /// <summary>
    /// A Method to delete the <see cref="List{T}"/> of <see cref="Podcast"/>
    /// Function has to be public to work. I don't know why!
    /// </summary>
    private async Task ResetAll()
    {
        _messenger.Send(new MessageData(false));
        DownloadService.CancelDownload = true;
        SetVariables();
        PodcastServices.DeletetAllImages();
        await DeleteAllAsync();
        await GetUpdatedPodcasts();
        await GetMostRecent();
        await GetDownloadedShows();
        await GetFavoriteShows();
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        PodcastServices.DeleleFiles(System.IO.Directory.GetFiles(path, "*.mp4"));

#if WINDOWS
        await MainThread.InvokeOnMainThreadAsync(async () => { await Shell.Current.GoToAsync($"{nameof(PodcastPage)}"); });
#endif
#if IOS || ANDROID || MACCATALYST

        await MainThread.InvokeOnMainThreadAsync(async () => { await Shell.Current.GoToAsync($"{nameof(SettingsPage)}"); });
#endif
    }
    private void SetVariables()
    {
        Preferences.Default.Clear();
        FavoriteShows.Clear();
        Shows.Clear();
        Podcasts.Clear();
        MostRecentShows.Clear();
        DownloadedShows.Clear();
        App.MostRecentShows.Clear();
        _messenger.Send(new MessageData(false));
    }
    private static async Task DeleteAllAsync()
    {
        await App.PositionData.DeleteAll();
        await App.PositionData.DeleteAllPodcasts();
        await App.PositionData.DeleteAllDownloads();
        await App.PositionData.DeleteAllFavorites();
    }
    #endregion
}
