// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Extensions;
// Since MediaElement can't access .NET MAUI internals we have to copy this code here
// https://github.com/dotnet/maui/blob/main/src/Controls/src/Core/Platform/PageExtensions.cs
static class PageExtensions
{
    internal static Page GetCurrentPage(this Page currentPage)
    {
        if (currentPage.NavigationProxy.ModalStack.AsEnumerable().LastOrDefault() is Page modal)
        {
            return modal;
        }

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
