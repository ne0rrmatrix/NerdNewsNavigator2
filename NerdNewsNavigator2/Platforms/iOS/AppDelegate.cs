// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Foundation;
using Microsoft.Maui.Handlers;
using SQLitePCL;
using UIKit;

namespace NerdNewsNavigator2;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    // Next line is for SqlLite
    protected override MauiApp CreateMauiApp()
    {
        raw.SetProvider(new SQLite3Provider_sqlite3());
        return MauiProgram.CreateMauiApp();
    }

    // iOS Bug fix START

    public AppDelegate() : base()
    {
    }
}
