// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application = Microsoft.Maui.Controls.Application;
using Platform = Microsoft.Maui.ApplicationModel.Platform;

#if ANDROID
using Views = AndroidX.Core.View;
#endif

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT;
using Microsoft.Maui.Controls;
#endif

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that manages showing a <see cref="List{T}"/> of downloaded <see cref="Show"/> to users.
/// </summary>
public partial class DownloadedShowPage : ContentPage, IRecipient<DeletedItemMessage>
{
    /// <summary>
    /// Initializes an instance of <see cref="DownloadedShowPage"/>
    /// </summary>
    /// <param name="viewModel">this classes <see cref="ViewModel"/> from <see cref="DownloadedShowViewModel"/></param>
    public DownloadedShowPage(DownloadedShowViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        WeakReferenceMessenger.Default.Register<DeletedItemMessage>(this);
    }

    /// <summary>
    /// Method recieves <see cref="DeletedItemMessage"/> and invokes <see cref="MessagingService.RecievedDelete(bool)"/>
    /// </summary>
    /// <param name="message"></param>
    public void Receive(DeletedItemMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await MessagingService.RecievedDelete(message.Value);
            WeakReferenceMessenger.Default.Register<DeletedItemMessage>(this);
        });
    }

#if WINDOWS
    /// <summary>
    /// Method is required for switching Full Screen Mode for Windows
    /// </summary>
    private Microsoft.UI.Windowing.AppWindow GetAppWindow(MauiWinUIWindow window)
    {
        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
        return appWindow;
    }
#endif

    /// <summary>
    /// Method toggles Full Screen On/Off
    /// </summary>

#nullable enable
    public void RestoreScreen()
    {
#if WINDOWS
        var window = GetParentWindow().Handler.PlatformView as MauiWinUIWindow;
        if (window is not null)
        {
            var appWindow = GetAppWindow(window);

            switch (appWindow.Presenter)
            {
                case Microsoft.UI.Windowing.OverlappedPresenter overlappedPresenter:
                    if (overlappedPresenter.State == Microsoft.UI.Windowing.OverlappedPresenterState.Maximized)
                    {
                        overlappedPresenter.SetBorderAndTitleBar(true, true);
                        overlappedPresenter.Restore();
                    }
                    break;
            }
        }
#endif
#if ANDROID
        var activity = Platform.CurrentActivity;

        if (activity == null || activity.Window == null) return;

        Views.WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);
        var windowInsetsControllerCompat = Views.WindowCompat.GetInsetsController(activity.Window, activity.Window.DecorView);
        var types = Views.WindowInsetsCompat.Type.StatusBars() |
                    Views.WindowInsetsCompat.Type.NavigationBars();
       
        //windowInsetsControllerCompat.SystemBarsBehavior = Views.WindowInsetsControllerCompat.BehaviorShowBarsBySwipe;
        windowInsetsControllerCompat.Show(types);
      
#endif
    }

    /// <summary>
    /// Method sets screen to normal screen size.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        RestoreScreen();
    }

#nullable disable

}
