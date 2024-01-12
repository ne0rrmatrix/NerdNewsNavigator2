// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using static Microsoft.Maui.ApplicationModel.Permissions;

namespace NerdNewsNavigator2.Services;
internal sealed class AndroidPermissions : BasePlatformPermission
{
#if ANDROID
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new List<(string androidPermission, bool isRuntime)>
        {
        (global::Android.Manifest.Permission.ForegroundService, true),
        (global::Android.Manifest.Permission.PostNotifications, true),
        }.ToArray();
#endif
}
