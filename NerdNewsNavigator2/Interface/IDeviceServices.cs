// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if WINDOWS
using Microsoft.UI.Windowing;
#endif

namespace NerdNewsNavigator2.Interface;
interface IDeviceServices
{
#if WINDOWS
    public AppWindow FullScreen();
    public AppWindow RestoreScreen();
#else
    public void FullScreen();
    public void RestoreScreen();
#endif
}
