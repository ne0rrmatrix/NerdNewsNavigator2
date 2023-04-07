// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that manages showing a <see cref="List{T}"/> of <see cref="Show"/> to users.
/// </summary>
public partial class ShowPage : ContentPage, IRecipient<InternetItemMessage>, IRecipient<DownloadItemMessage>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShowPage"/> class.
    /// </summary>
    /// <param name="viewModel">This pages <see cref="ViewModel"/> from <see cref="ShowViewModel"/></param>
    public ShowPage(ShowViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        WeakReferenceMessenger.Default.Register<DownloadItemMessage>(this);
        WeakReferenceMessenger.Default.Register<InternetItemMessage>(this);
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
    /// Method invokes <see cref="MessagingService.RecievedDownloadMessage(bool)"/> for display a <see cref="Toast"/>
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
}
