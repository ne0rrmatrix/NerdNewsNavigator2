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
        OnPropertyChanged(nameof(IsBusy));
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
        await App.PositionData.DeleteAll();
        await App.PositionData.DeleteAllPodcasts();
        await App.PositionData.DeleteAllDownloads();
        await App.PositionData.DeleteAllFavorites();

        var start = Preferences.Default.Get("start", false);
        if (start)
        {
            _messenger.Send(new MessageData(false));
        }

        Preferences.Default.Clear();
        FavoriteShows.Clear();
        Shows.Clear();
        Podcasts.Clear();

        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var files = System.IO.Directory.GetFiles(path, "*.mp4");
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

        IsBusy = false;
        await Shell.Current.GoToAsync($"{nameof(PodcastPage)}");
    }
}
