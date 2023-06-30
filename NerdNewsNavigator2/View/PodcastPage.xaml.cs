// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that displays a <see cref="List{T}"/> of <see cref="Podcast"/> from twit.tv network.
/// </summary>
public partial class PodcastPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastPage"/> class.
    /// </summary>
    /// <param name="viewModel">This pages <see cref="ViewModel"/> from <see cref="PodcastViewModel"/></param>
    public PodcastPage(PodcastViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>
    /// Method sets screen to normal screen size.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        if (DeviceDisplay.Current.MainDisplayInfo.Width <= 1920 && DeviceInfo.Current.Platform == DevicePlatform.WinUI)
        {
            ItemLayout.Span = 2;
            OnPropertyChanged(nameof(ItemLayout));
        }
        DeviceService.RestoreScreen();
#if WINDOWS|| MACCATALYST
        if (DownloadService.IsDownloading)
        {
            Shell.SetNavBarIsVisible(Shell.Current.CurrentPage, true);
        }
#endif
    }
}
