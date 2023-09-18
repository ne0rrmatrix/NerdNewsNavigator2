// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Android.App;
using Android.OS;
using Android.Runtime;

namespace NerdNewsNavigator2.Platforms.Android;

[Application]
public class MainApplication(nint handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership)
{
    public static readonly string ChannelId = "autoDownloadServiceChannel";

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    public override void OnCreate()
    {
        base.OnCreate();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var serviceChannel =
               new NotificationChannel(ChannelId, "AutoDownload Service Channel", NotificationImportance.None);

            if (GetSystemService(NotificationService) is NotificationManager manager)
            {
                manager.CreateNotificationChannel(serviceChannel);
            }
        }
    }
}
