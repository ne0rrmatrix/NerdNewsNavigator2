// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UIKit;

#pragma warning disable IDE1006 // Naming Styles
namespace NerdNewsNavigator2.Platforms.iOS;
#pragma warning restore IDE1006 // Naming Styles
public static class Program
{
    // This is the main entry point of the application.
    private static void Main(string[] args)
    {
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
