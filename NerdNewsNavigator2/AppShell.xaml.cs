// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2;
/// <summary>
/// A class that inherits <see cref="Shell"/> and manages it.
/// </summary>
public partial class AppShell : Shell
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppShell"/> class.
    /// </summary>
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(PodcastPage), typeof(PodcastPage));
        Routing.RegisterRoute(nameof(ShowPage), typeof(ShowPage));
        Routing.RegisterRoute(nameof(VideoPlayerPage), typeof(VideoPlayerPage));
        Routing.RegisterRoute(nameof(LivePage), typeof(LivePage));
        Routing.RegisterRoute(nameof(EditPage), typeof(EditPage));
        Routing.RegisterRoute(nameof(ResetAllSettingsPage), typeof(ResetAllSettingsPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(MostRecentShowsPage), typeof(MostRecentShowsPage));
        Routing.RegisterRoute(nameof(DownloadedShowPage), typeof(DownloadedShowPage));
    }

    #region Navigation
    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        WeakReferenceMessenger.Default.Send(new NavigatedItemMessage(true));
        base.OnNavigated(args);
    }
    /// <summary>
    /// Method navigates user to Main Page.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GotoFirstPage(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"{nameof(PodcastPage)}");
    }

    /// <summary>
    /// Method navigates user to Main Page.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GotoMostRecentShowPage(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"{nameof(MostRecentShowsPage)}");
    }

    /// <summary>
    /// Method resets database.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Reset(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(ResetAllSettingsPage)}");
    }

    /// <summary>
    /// Method navigates user to Live Video for twit.tv
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GotoLivePage(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"{nameof(LivePage)}");
    }

    /// <summary>
    /// Method navigates user to Page that allows you to add a podcast.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GotoAddPage(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"{nameof(SettingsPage)}");
    }
    /// <summary>
    /// Method navigates user to Page that allows you to add a podcast.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GotoDownloadedPage(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"{nameof(DownloadedShowPage)}");
    }
    #endregion
}
