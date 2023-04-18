// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;
/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="UpdateSettingsPage"/>
/// </summary>
public partial class UpdateSettingsViewModel : BaseViewModel
{
    private readonly ILogger<UpdateSettingsViewModel> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSettingsViewModel"/> class.
    /// </summary>
    public UpdateSettingsViewModel(ILogger<UpdateSettingsViewModel> logger, IConnectivity connectivity) : base(logger, connectivity)
    {
        _logger = logger;
        Shell.Current.FlyoutIsPresented = false;
        OnPropertyChanged(nameof(IsBusy));
        _ = DeleteAllPodcasts();
    }

    /// <summary>
    /// A Method to delete the <see cref="List{T}"/> of <see cref="Podcast"/>
    /// Function has to be public to work. I don't know why!
    /// </summary>
    private async Task DeleteAllPodcasts()
    {
        IsBusy = true;
        while (IsBusy)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var files = System.IO.Directory.GetFiles(path, "*.mp4");
            if (files.Any() && files is not null)
            {
                foreach (var file in files)
                {
                    System.IO.File.Delete(file);
                    _logger.LogInformation("Deleted file {file}", file);
                }
            }
            await App.PositionData.DeleteAll();
            await App.PositionData.DeleteAllPodcasts();
            await App.PositionData.DeleteAllDownloads();
            await App.PositionData.DeleteAllFavorites();
            FavoriteShows.Clear();
            Shows.Clear();
            Podcasts.Clear();
            await Task.Run(async () =>
            {
                await GetUpdatedPodcasts();
            });
            IsBusy = false;
        }
        await Shell.Current.GoToAsync($"{nameof(PodcastPage)}");
    }
}
