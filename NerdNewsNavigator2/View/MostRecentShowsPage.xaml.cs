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
    /// Method displays a <see cref="Toast"/> about status of deleted files.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private async Task RecievedDownloadMessage(bool value)
    {
        if (value)
        {

            await Toast.Make("Download is completed.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        }
        else
        {

            await Toast.Make("Download Failed!", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        }
        WeakReferenceMessenger.Default.Reset();
        WeakReferenceMessenger.Default.Register<DownloadItemMessage>(this);
    }

    /// <summary>
    /// Method displays a <see cref="Toast"/> about status of internet.
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
    /// Method display a <see cref="Toast"/> about status of download starting.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Toast.Make("Added show to downloads.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        WeakReferenceMessenger.Default.Reset();
    }

    /// <summary>
    /// Method invokes <see cref="RecievedDownloadMessage(bool)"/> for displaying <see cref="Toast"/>
    /// </summary>
    /// <param name="message"></param>
    public void Receive(DownloadItemMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await RecievedDownloadMessage(message.Value);
        });
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
}
