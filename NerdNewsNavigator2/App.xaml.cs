// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Maui;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;
using System.Threading;
#if WINDOWS
using NerdNewsNavigator2.WinUI;
#endif
#if ANDROID
using NerdNewsNavigator2.Platforms.Android;
#endif
namespace NerdNewsNavigator2;

/// <summary>
/// A class that acts as a manager for <see cref="Application"/>
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// This applications Dependancy Injection for <see cref="PositionDataBase"/> class.
    /// </summary>
    public static PositionDataBase PositionData { get; private set; }

    private readonly IMessenger _messenger;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="positionDataBase"></param>
    public App(PositionDataBase positionDataBase, IMessenger messenger)
    {
        InitializeComponent();
#if ANDROID
        // Local Notification tap event listener
        LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;
#endif
        MainPage = new AppShell();

        _messenger = messenger;
        // Database Dependancy Injection START
        PositionData = positionDataBase;
        // Database Dependancy Injection END

        LogController.InitializeNavigation(
            page => MainPage!.Navigation.PushModalAsync(page),
            () => MainPage!.Navigation.PopModalAsync());
        _ = ThreadPool.QueueUserWorkItem(state =>
        {
            StartAutoDownloadService();
        });
    }
#nullable enable
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Destroying += (s, e) =>
        {
#if WINDOWS
            if (WinUI.App.CancellationTokenSource is null)
            {
                Debug.WriteLine("Cancellation Token already disposed");
            }
            else if (WinUI.App.CancellationTokenSource is not null)
            {
                Debug.WriteLine("Stopping AutoDownload");
                WinUI.App.CancellationTokenSource.Cancel();
                WinUI.App.LongTask(WinUI.App.CancellationTokenSource.Token);
                WinUI.App.CancellationTokenSource?.Dispose();
                WinUI.App.CancellationTokenSource = null;
                Debug.WriteLine("Disposed of Cancellation Token");
            }
#endif
#if ANDROID

            if (AutoStartService.CancellationTokenSource is null)
            {
                Debug.WriteLine("Cancellation Token already disposed");
            }
            else if (AutoStartService.CancellationTokenSource is not null)
            {
                Debug.WriteLine("Stopping AutoDownload");
                AutoStartService.CancellationTokenSource.Cancel();
                AutoStartService.LongTask(AutoStartService.CancellationTokenSource.Token);
                AutoStartService.CancellationTokenSource?.Dispose();
                AutoStartService.CancellationTokenSource = null;
                Debug.WriteLine("Disposed of Cancellation Token");
            }
#endif
        };

        return window;
    }
#nullable disable

#if ANDROID
    private async void OnNotificationActionTapped(NotificationActionEventArgs e)
    {
        if (e.IsTapped)
        {
            await Shell.Current.GoToAsync($"{nameof(DownloadedShowPage)}");
        }
    }
#endif
    private void StartAutoDownloadService()
    {
        Thread.Sleep(5000);
        var start = Preferences.Default.Get("start", true);
        if (start)
        {
            _messenger.Send(new MessageData(true));
        }
        else
        {
            Debug.WriteLine("error");
        }
    }
}

