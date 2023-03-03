// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that manages showing a <see cref="List{T}"/> of <see cref="Show"/> to users.
/// </summary>
public partial class TabletShowPage : ContentPage, IRecipient<InternetItemMessage>, IRecipient<DownloadItemMessage>
{
    MessagingService MessagingS { get; set; } = new();
    /// <summary>
    /// Initializes a new instance of the <see cref="TabletShowPage"/> class.
    /// </summary>
    /// <param name="viewModel">This pages <see cref="ViewModel"/> from <see cref="TabletShowViewModel"/></param>
    public TabletShowPage(TabletShowViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        WeakReferenceMessenger.Default.Register<DownloadItemMessage>(this);
    }

    /// <summary>
    /// Method invokes <see cref="RecievedInternetMessage(bool)"/> for displaying <see cref="Toast"/>
    /// </summary>
    /// <param name="message"></param>
    public void Receive(InternetItemMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await MessagingS.RecievedInternetMessage(message.Value);
        });
    }

    /// <summary>
    /// Method invokes <see cref="RecievedDownloadSucess(bool)"/> for display a <see cref="Toast"/>
    /// </summary>
    /// <param name="message"></param>
    public void Receive(DownloadItemMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await MessagingS.RecievedDownloadMessage(message.Value);
            WeakReferenceMessenger.Default.Register<DownloadItemMessage>(this);
        });
    }
}
