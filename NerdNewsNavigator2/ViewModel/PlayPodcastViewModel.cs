// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NerdNewsNavigator2.View;

namespace NerdNewsNavigator2.ViewModel;

[QueryProperty("Url", "Url")]
public partial class PlayPodcastViewModel : ObservableObject
{
    #region Properties
    [ObservableProperty]
    public string url; //# Observable properties have to be lower case
    #endregion

    [RelayCommand]
    async Task SwipeGesture(string Url) => await Shell.Current.GoToAsync($"{nameof(ShowPage)}?Url={Url}");
}

