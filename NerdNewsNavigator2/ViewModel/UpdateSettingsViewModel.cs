// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;
/// <summary>
/// A class that inherits from <see cref="BaseViewModel"/> and manages <see cref="UpdateSettingsPage"/>
/// </summary>
public partial class UpdateSettingsViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSettingsViewModel"/> class.
    /// </summary>
    public UpdateSettingsViewModel()
    {
        Shell.Current.FlyoutIsPresented = false;
        IsBusy = true;
        OnPropertyChanged(nameof(IsBusy));
        DeleteAllPodcasts();
    }
    /// <summary>
    /// A Method to delete the <see cref="List{T}"/> of <see cref="Podcast"/>
    /// </summary>
    private async void DeleteAllPodcasts()
    {
        try
        {
            while (IsBusy)
            {
                await App.PositionData.DeleteAll();
                await App.PositionData.DeleteAllPodcasts();
                await Shell.Current.GoToAsync($"{nameof(TabletPodcastPage)}");
                IsBusy = false;
            }
        }
        catch { }
    }
}
