// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;
internal static partial class DeviceService
{
    public static partial void FullScreen()
    {

#if IOS || MACCATALYST
#pragma warning disable CA1422 // Validate platform compatibility
        UIKit.UIApplication.SharedApplication.SetStatusBarHidden(true, UIKit.UIStatusBarAnimation.Fade);
#pragma warning restore CA1422 // Validate platform compatibility
#endif
    }
    public static partial void RestoreScreen()
    {

#if IOS || MACCATALYST
#pragma warning disable CA1422 // Validate platform compatibility
        UIKit.UIApplication.SharedApplication.SetStatusBarHidden(false, UIKit.UIStatusBarAnimation.None);
#pragma warning restore CA1422 // Validate platform compatibility
#endif
    }
}
