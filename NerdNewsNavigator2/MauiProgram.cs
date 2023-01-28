// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if WINDOWS
                        using Microsoft.UI;
                       using Microsoft.UI.Windowing;
                        using Windows.Graphics;
#endif

namespace NerdNewsNavigator2;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        }).UseMauiCommunityToolkit().UseMauiCommunityToolkitMediaElement();

#if WINDOWS
 builder.ConfigureLifecycleEvents(events =>  
        {  
            events.AddWindows(wndLifeCycleBuilder =>  
            {  
                wndLifeCycleBuilder.OnWindowCreated(window =>  
                {  
                    window.ExtendsContentIntoTitleBar = false;  
                    IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);  
                    WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);  
                    var _appWindow = AppWindow.GetFromWindowId(myWndId);  
                    _appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);                          
                });  
            });  
        });  
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<PodcastPage>();
        builder.Services.AddSingleton<PodcastViewModel>();

        builder.Services.AddTransient<ShowPage>();
        builder.Services.AddTransient<ShowViewModel>();

        builder.Services.AddTransient<PlayPodcastPage>();
        builder.Services.AddTransient<PlayPodcastViewModel>();

        builder.Services.AddTransient<LivePage>();
        builder.Services.AddTransient<LiveViewModel>();

        builder.Services.AddSingleton<TwitService>();
        builder.Services.AddSingleton<FeedService>();
        return builder.Build();
    }
}
