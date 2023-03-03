// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that manages showing a <see cref="List{T}"/> of downloaded <see cref="Show"/> to users.
/// </summary>
public partial class DownloadedShowPage : ContentPage, IRecipient<DeletedItemMessage>
{
    /// <summary>
    /// Initializes an instance of <see cref="DownloadedShowPage"/>
    /// </summary>
    /// <param name="viewModel">this classes <see cref="ViewModel"/> from <see cref="DownloadedShowViewModel"/></param>
    public DownloadedShowPage(DownloadedShowViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        WeakReferenceMessenger.Default.Register<DeletedItemMessage>(this);
    }

    /// <summary>
    /// Method recieves <see cref="DeletedItemMessage"/> and invokes <see cref="RecievedDelete(bool)"/>
    /// </summary>
    /// <param name="message"></param>
    public void Receive(DeletedItemMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await RecievedDelete(message.Value);
        });
    }

    /// <summary>
    /// Method displays a <see cref="Toast"/> about status of deleted files.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private async Task RecievedDelete(bool value)
    {
        if (value)
        {
            await Toast.Make("Download is Deleted.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        }
        else
        {
            await Toast.Make("Failed to Delete Download.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        }
        WeakReferenceMessenger.Default.Reset();
        WeakReferenceMessenger.Default.Register<DeletedItemMessage>(this);
    }
}
