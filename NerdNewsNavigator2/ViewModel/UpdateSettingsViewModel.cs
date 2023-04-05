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
        try
        {
            IsBusy = true;
            while (IsBusy)
            {
                var temp = await App.PositionData.GetAllDownloads();
                if (temp is null)
                {
                    _logger.LogInformation("Did not find any files to delete.");
                }
                else
                {
                    foreach (var (item, tempFile) in from item in temp
                                                     let tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), item.FileName)
                                                     where File.Exists(tempFile)
                                                     select (item, tempFile))
                    {
                        File.Delete(tempFile);
                        _logger.LogInformation("Deleted {file}", item.FileName);
                    }
                }

                await App.PositionData.DeleteAll();
                await App.PositionData.DeleteAllPodcasts();
                await App.PositionData.DeleteAllDownloads();
                Shows.Clear();
                Podcasts.Clear();
                await Shell.Current.GoToAsync($"{nameof(PodcastPage)}");
                IsBusy = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Delete podcasts failed. {Message}", ex.Message);
        }
    }
}
