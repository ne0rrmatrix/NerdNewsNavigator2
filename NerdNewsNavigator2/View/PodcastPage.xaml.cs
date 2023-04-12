// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Messaging;

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that displays a <see cref="List{T}"/> of <see cref="Podcast"/> from twit.tv network.
/// </summary>
public partial class PodcastPage : ContentPage, IRecipient<InternetItemMessage>, IRecipient<DownloadItemMessage>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastPage"/> class.
    /// </summary>
    /// <param name="viewModel">This pages <see cref="ViewModel"/> from <see cref="PodcastViewModel"/></param>
    public PodcastPage(PodcastViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        WeakReferenceMessenger.Default.Register<DownloadItemMessage>(this);
        WeakReferenceMessenger.Default.Register<InternetItemMessage>(this);
    }

    /// <summary>
    /// Method invokes <see cref="MessagingService.RecievedDownloadMessage(bool)"/> for displaying <see cref="Toast"/>
    /// </summary>
    /// <param name="message"></param>
    public void Receive(DownloadItemMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await MessagingService.RecievedDownloadMessage(message.Value);
            WeakReferenceMessenger.Default.Register<DownloadItemMessage>(this);
        });
    }

    /// <summary>
    /// Method invokes <see cref="MessagingService.RecievedInternetMessage(bool)"/> for displaying <see cref="Toast"/>
    /// </summary>
    /// <param name="message"></param>
    public void Receive(InternetItemMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await MessagingService.RecievedInternetMessage(message.Value);
        });
    }

    /// <summary>
    /// Method sets screen to normal screen size.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        DeviceService.RestoreScreen();
        if (App.IsDownloading)
        {
            Shell.SetNavBarIsVisible(Shell.Current.CurrentPage, true);
        }
    }
}
