// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that manages showing a <see cref="List{T}"/> of <see cref="Show"/> to users.
/// </summary>
public partial class TabletShowPage : ContentPage, IRecipient<InternetItemMessage>, IRecipient<DownloadItemMessage>
{
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
    /// Method displays a <see cref="Toast"/> about status of downloaded files.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private async Task RecievedDownloadSucess(bool value)
    {
        if (value)
        {
            await Toast.Make("Download is completed.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        }
        else
        {

            await Toast.Make("Download Failed.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        }
        WeakReferenceMessenger.Default.Reset();
        WeakReferenceMessenger.Default.Register<DownloadItemMessage>(this);
    }

    /// <summary>
    /// Method display a <see cref="Toast"/> about status of Internet.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private async Task RecievedInternetMessage(bool value)
    {
        if (!value)
        {
            await Toast.Make("Can't Connect to Internet.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            WeakReferenceMessenger.Default.Reset();
        }
    }

    /// <summary>
    /// Method invokes <see cref="RecievedInternetMessage(bool)"/> for displaying <see cref="Toast"/>
    /// </summary>
    /// <param name="message"></param>
    public void Receive(InternetItemMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await RecievedInternetMessage(message.Value);
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
            await RecievedDownloadSucess(message.Value);
        });
    }
}
