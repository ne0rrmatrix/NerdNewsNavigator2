// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;
/// <summary>
/// A class to manage Messaging between classes.
/// </summary>
public static class MessagingService
{
    /// <summary>
    /// Method displays a <see cref="Toast"/> about status of deleted files.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task RecievedDownloadMessage(bool value)
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
    }

    /// <summary>
    /// Method displays a <see cref="Toast"/> about status of internet.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task RecievedInternetMessage(bool value)
    {
        if (!value)
        {
            await Toast.Make("Can't Connect to Internet.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            WeakReferenceMessenger.Default.Reset();
        }
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
            await Toast.Make("Download is Deleted.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        }
        else
        {
            await Toast.Make("Failed to Delete Download.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        }
        WeakReferenceMessenger.Default.Reset();
    }
}
