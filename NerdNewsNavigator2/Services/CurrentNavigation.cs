// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NavigationEventArgs = NerdNewsNavigator2.Primitives.NavigationEventArgs;

namespace NerdNewsNavigator2.Services;
public partial class CurrentNavigation : ObservableObject
{
    [ObservableProperty]
    private bool _isNavigating;
    [ObservableProperty]
    private bool _isShow;
    public EventHandler<NavigationEventArgs> NavigationCompleted { get; set; }
    public CurrentNavigation()
    {
        IsNavigating = true;
        IsShow = true;
    }
    public void StartedNavigation(bool isNavigating, bool isShows)
    {
        var args = new NavigationEventArgs
        {
            IsNavigating = isNavigating,
            IsShows = isShows
        };
        OnStarted(args);
    }
    protected virtual void OnStarted(NavigationEventArgs args)
    {
        NavigationCompleted?.Invoke(this, args);
    }

}
