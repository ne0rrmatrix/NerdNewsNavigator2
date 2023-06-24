// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A Class that extends <see cref="BaseViewModel"/> for <see cref="SettingsViewModel"/>
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    private readonly IFileSaver _fileSaver;
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/>
    /// </summary>
    public SettingsViewModel(ILogger<SettingsViewModel> logger, IConnectivity connectivity, IFileSaver fileSaver)
        : base(logger, connectivity)
    {
        if (DownloadService.IsDownloading)
        {
            ThreadPool.QueueUserWorkItem(state => { UpdatingDownload(); });
        }
        _fileSaver = fileSaver;
    }
    /// <summary>
    /// A Method that passes a Url <see cref="string"/> to <see cref="ShowPage"/>
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public Task UpdatePodcasts()
    {
        ThreadPool.QueueUserWorkItem(async (state) =>
       {
           _ = await PodcastServices.UpdatePodcast();
           Podcasts.Clear();

           _ = await PodcastServices.UpdateFavoritesAsync();
           FavoriteShows.Clear();

           await MainThread.InvokeOnMainThreadAsync(async () =>
           {
               await Shell.Current.GoToAsync($"{nameof(PodcastPage)}");
           });
       });
        return Task.CompletedTask;
    }

    /// <summary>
    /// Method checks for required Permission for Android Notifications and requests them if needed
    /// </summary>
    /// <returns></returns>
    public static async Task<PermissionStatus> CheckAndRequestForeGroundPermission()
    {
        var status = await Permissions.CheckStatusAsync<AndroidPermissions>();
        if (status == PermissionStatus.Granted)
        {
            return status;
        }
        else
        {
            await Shell.Current.DisplayAlert("Permission Required", "Notification permission is required for Auto Downloads to work in background. It runs on an hourly schedule.", "Ok");
        }
        status = await Permissions.RequestAsync<AndroidPermissions>();
        return status;
    }

    [RelayCommand]
    public static void Shake()
    {
#pragma warning disable IDE0017 // Simplify object initialization
        var test = new LogController();
#pragma warning restore IDE0017 // Simplify object initialization
        test.IsShakeEnabled = true;
    }

    [RelayCommand]
    public async Task SaveFile(CancellationToken cancellationToken)
    {
        _ = await CheckAndRequestForeGroundPermission();
        if (!LogOperatorRetriever.Instance.TryGetOperator<ILogCompressor>(out var logCompressor))
        {
            return;
        }
        var stream = logCompressor.GetCompressedLogs().Result;
        try
        {
            var fileName = Application.Current?.MainPage?.DisplayPromptAsync("FileSaver", "Choose filename") ?? Task.FromResult("debug.txt");
            var fileLocationResult = await _fileSaver.SaveAsync(await fileName, stream, cancellationToken);
            fileLocationResult.EnsureSuccess();

            await Toast.Make($"File is saved: {fileLocationResult.FilePath}").Show(cancellationToken);
        }
        catch (Exception ex)
        {
            await Toast.Make($"File is not saved, {ex.Message}").Show(cancellationToken);
        }
    }
}
