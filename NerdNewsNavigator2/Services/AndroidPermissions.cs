// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NerdNewsNavigator2.Platforms.Android;

namespace NerdNewsNavigator2.Services;

public class AndroidPermissions : Permissions.BasePermission
{
    public override async Task<PermissionStatus> CheckStatusAsync()
    {
        //throw new NotImplementedException();
        var status = await Permissions.CheckStatusAsync<ForegroundService>();
        return status;
    }

    public override void EnsureDeclared()
    {
        throw new NotImplementedException();
    }

    public override async Task<PermissionStatus> RequestAsync()
    {
        //throw new NotImplementedException();
        var status = await Permissions.RequestAsync<ForegroundService>();
        return status;
    }

    public override bool ShouldShowRationale()
    {
        //throw new NotImplementedException();
        return Permissions.ShouldShowRationale<ForegroundService>();
    }
}
