// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Extensions;
// Since MediaElement can't access .NET MAUI internals we have to copy this code here
// https://github.com/dotnet/maui/blob/main/src/Controls/src/Core/Platform/PageExtensions.cs
static class PageExtensions
{
    private static bool s_navBarIsVisible = false;
    private static bool s_tabBarIsVisible = false;
    private static bool s_backButton = false;
    private static string s_backButtonTitle = string.Empty;
    private static string s_pageTitle = string.Empty;
    static Page CurrentPage => GetCurrentPage(Application.Current?.MainPage ?? throw new InvalidOperationException($"{nameof(Application.Current.MainPage)} cannot be null."));
    public static void SetBarStatus(bool shouldBeFullScreen)
    {
#if IOS || MACCATALYST
#pragma warning disable CA1422 // Validate platform compatibility
        UIKit.UIApplication.SharedApplication.SetStatusBarHidden(shouldBeFullScreen, UIKit.UIStatusBarAnimation.Fade);
#pragma warning restore CA1422 // Validate platform compatibility
#endif
        // let's cache the CurrentPage here, since the user can navigate or background the app
        // while this method is running
        var currentPage = CurrentPage;

        if (shouldBeFullScreen)
        {
            s_navBarIsVisible = Shell.GetNavBarIsVisible(currentPage);
            s_tabBarIsVisible = Shell.GetTabBarIsVisible(currentPage);
            s_backButton = NavigationPage.GetHasBackButton(currentPage);
            s_backButtonTitle = NavigationPage.GetBackButtonTitle(currentPage);
            NavigationPage.SetBackButtonTitle(currentPage, string.Empty);
            NavigationPage.SetHasBackButton(currentPage, false);
            s_pageTitle = currentPage.Title;
            currentPage.Title = string.Empty;
            Shell.SetNavBarIsVisible(currentPage, false);
            Shell.SetTabBarIsVisible(currentPage, false);
            NavigationPage.SetHasNavigationBar(currentPage, false);
        }
        else
        {
            NavigationPage.SetHasNavigationBar(currentPage, s_navBarIsVisible);
            NavigationPage.SetHasBackButton(currentPage, s_backButton);
            NavigationPage.SetBackButtonTitle(currentPage, s_backButtonTitle);
            Shell.SetNavBarIsVisible(currentPage, s_navBarIsVisible);
            Shell.SetTabBarIsVisible(currentPage, s_tabBarIsVisible);
            currentPage.Title = s_pageTitle;
        }
    }
    internal static Page GetCurrentPage(this Page currentPage)
    {
#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections
        if (currentPage.NavigationProxy.ModalStack.LastOrDefault() is Page modal)
        {
            return modal;
        }
#pragma warning restore CA1826 // Do not use Enumerable methods on indexable collections

        if (currentPage is FlyoutPage flyoutPage)
        {
            return GetCurrentPage(flyoutPage.Detail);
        }

        if (currentPage is Shell shell && shell.CurrentItem?.CurrentItem is IShellSectionController shellSectionController)
        {
            return shellSectionController.PresentedPage;
        }

        if (currentPage is IPageContainer<Page> paigeContainer)
        {
            return GetCurrentPage(paigeContainer.CurrentPage);
        }

        return currentPage;
    }
}
