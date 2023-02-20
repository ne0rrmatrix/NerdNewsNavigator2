﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;
public partial class UpdateSettingsViewModel : BaseViewModel
{
    public UpdateSettingsViewModel()
    {
        Shell.Current.FlyoutIsPresented = false;
        IsBusy = true;
        OnPropertyChanged(nameof(IsBusy));
        DeleteAllPodcasts();
    }
    private async void DeleteAllPodcasts()
    {
        while (IsBusy)
        {
            await App.PositionData.GetAllPositions();
            await App.PositionData.DeleteAll();
            await App.PositionData.DeleteAllPodcasts();
            await Shell.Current.GoToAsync($"{nameof(TabletPodcastPage)}");
            IsBusy = false;
        }
    }
}
