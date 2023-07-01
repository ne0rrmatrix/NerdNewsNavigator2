﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;
/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="ResetAllSettingsViewModel"/>
/// </summary>
public partial class ResetAllSettingsViewModel : BaseViewModel
{
    private readonly IMessenger _messenger;
    /// <summary>
    /// An <see cref="ILogger{TCategoryName}"/> instance managed by this class.
    /// </summary>
    private readonly ILogger<ResetAllSettingsViewModel> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetAllSettingsViewModel"/> class.
    /// </summary>
    public ResetAllSettingsViewModel(ILogger<ResetAllSettingsViewModel> logger, IConnectivity connectivity, IMessenger messenger) : base(logger, connectivity)
    {
        _logger = logger;
        Shell.Current.FlyoutIsPresented = false;
        _ = ResetAll();
        _messenger = messenger;
    }

    /// <summary>
    /// A Method to delete the <see cref="List{T}"/> of <see cref="Podcast"/>
    /// Function has to be public to work. I don't know why!
    /// </summary>
    private async Task ResetAll()
    {
        DownloadService.CancelDownload = true;
        IsBusy = true;
        await DeleteAllAsync();
        SetVariables();

        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        DeleleFiles(System.IO.Directory.GetFiles(path, "*.mp4"));
        _ = Task.Run(async () =>
        {
            await GetUpdatedPodcasts();
            IsBusy = false;
            ThreadPool.QueueUserWorkItem(GetDownloadedShows);
            ThreadPool.QueueUserWorkItem(GetFavoriteShows);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Shell.Current.GoToAsync($"{nameof(PodcastPage)}");
            });
        });
    }
    private void DeleleFiles(string[] files)
    {
        try
        {
            foreach (var file in files)
            {
                System.IO.File.Delete(file);
                _logger.LogInformation("Deleted file {file}", file);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation("{data}", ex.Message);
        }
    }
    private void SetVariables()
    {
        Preferences.Default.Clear();
        FavoriteShows.Clear();
        Shows.Clear();
        Podcasts.Clear();
        _messenger.Send(new MessageData(false));
    }
    private static async Task DeleteAllAsync()
    {
        await App.PositionData.DeleteAll();
        await App.PositionData.DeleteAllPodcasts();
        await App.PositionData.DeleteAllDownloads();
        await App.PositionData.DeleteAllFavorites();
    }
}
