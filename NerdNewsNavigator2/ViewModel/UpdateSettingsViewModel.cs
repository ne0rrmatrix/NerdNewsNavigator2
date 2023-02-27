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
        _logger = logger;
    }
    private async void Next(object stateInfo)
    {
        await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync($"{nameof(TabletPodcastPage)}"));
    }
    /// <summary>
    /// A Method to delete the <see cref="List{T}"/> of <see cref="Podcast"/>
    /// </summary>
    private async void DeleteAllPodcasts(object stateInfo)
    {
        try
        {
            while (IsBusy)
            {
                var temp = await App.PositionData.GetAllDownloads();
                if (temp is not null)
                {
                    foreach (var item in temp)
                    {

                        var tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), item.FileName);
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                            _logger.LogInformation("Deleted {file}", item.FileName);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Did not find any files to delete.");
                }

                await App.PositionData.DeleteAll();
                await App.PositionData.DeleteAllPodcasts();
                await App.PositionData.DeleteAllDownloads();
                Shows.Clear();
                Podcasts.Clear();
                IsBusy = false;
                ThreadPool.QueueUserWorkItem(Next);
            }
        }
        catch { }
    }
}
