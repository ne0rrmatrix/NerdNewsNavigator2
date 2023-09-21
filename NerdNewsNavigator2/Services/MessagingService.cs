// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NerdNewsNavigator2.Extensions;

namespace NerdNewsNavigator2.Services;
/// <summary>
/// A class to manage Messaging between classes.
/// </summary>
public class MessagingService : IRecipient<InternetItemMessage>, IRecipient<DownloadItemMessage>
{
    /// <summary>
    /// Gets the presented page.
    /// </summary>
    protected static Page CurrentPage
    {
        get
        {
            return PageExtensions.GetCurrentPage(Application.Current?.MainPage ?? throw new InvalidOperationException($"{nameof(Application.Current.MainPage)} cannot be null."));
        }
    }

    public MessagingService()
    {
        WeakReferenceMessenger.Default.Register<DownloadItemMessage>(this);
        WeakReferenceMessenger.Default.Register<InternetItemMessage>(this);
    }

    /// <summary>
    /// Method displays a <see cref="Toast"/> about status of deleted files.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task RecievedDelete(bool value)
    {
        if (value)
        {
            await Toast.Make("Download is Deleted.", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
            return;
        }
        await Toast.Make("Failed to Delete Download.", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
    }

    public void Receive(DownloadItemMessage message)
    {
        WeakReferenceMessenger.Default.Unregister<DownloadItemMessage>(message);
        if (message.Value)
        {
            _ = Toast.Make($"Download {message.Title} is completed.", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
            return;
        }
        _ = Toast.Make($"Download {message.Title} Failed!", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
    }

    public void Receive(InternetItemMessage message)
    {
        WeakReferenceMessenger.Default.Unregister<InternetItemMessage>(message);
        if (!message.Value)
        {
            _ = Toast.Make("Can't Connect to Internet.", CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
        }
    }
}
