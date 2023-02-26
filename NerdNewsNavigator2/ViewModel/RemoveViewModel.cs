// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="RemovePage"/>
/// </summary>
public partial class RemoveViewModel : BaseViewModel
{
    private readonly ILogger<RemoveViewModel> _logger;
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveViewModel"/> instance.
    /// </summary>
    public RemoveViewModel(ILogger<RemoveViewModel> logger)
        : base(logger)
    {
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        this._orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
        _ = GetUpdatedPodcasts();
        _logger = logger;
    }

    /// <summary>
    /// Method Deletes a <see cref="Podcast"/> from <see cref="List{T}"/> <see cref="Podcast"/> in <see cref="BaseViewModel"/>
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Tap(string url)
    {
        var result = await PodcastServices.Delete(url);
        if (!result) { return; }
        foreach (var item in from item in Podcasts
                             where item.Url == url
                             select item)
        {
            if (Podcasts.Contains(item)) { Podcasts.Remove(item); _logger.LogInformation("Deleted podcast: {podcast}", item.Title); }
            break;
        }
    }

    /// <summary>
    /// Method Deletes a <see cref="Podcast"/> from <see cref="List{T}"/> <see cref="Podcast"/> in <see cref="BaseViewModel"/>
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task Download(string url)
    {
        foreach (var item in from item in Podcasts
                             where item.Url == url
                             select item)
        {
            if (Podcasts.Contains(item))
            {
                Podcasts.Remove(item);
                await App.PositionData.DeletePodcast(item);
                Podcast podcast = new()
                {
                    Url = url,
                    Title = item.Title,
                    Description = item.Description,
                    PubDate = item.PubDate,
                    Download = true
                };
                Podcasts.Add(podcast);
                await App.PositionData.AddPodcast(podcast);
                return;
            }
        }
    }
}
