// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that Manages the display of the most recent Shows from twit.tv
/// </summary>
public partial class MostRecentShowsPage : ContentPage, IRecipient<DownloadItemMessage>, IRecipient<InternetItemMessage>
{
    /// <summary>
    /// Initializes a new instance of <see cref="MostRecentShowsPage"/>
    /// </summary>
    /// <param name="viewmodel"></param>
    public MostRecentShowsPage(MostRecentShowsViewModel viewmodel)
    {
        InitializeComponent();
        BindingContext = viewmodel;
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
    }
}
