// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public bool _isBusy;
    public bool IsNotBusy => !IsBusy;

    [ObservableProperty]
    public int _orientation;
    public DisplayInfo MyMainDisplay { get; set; } = new();
    public BaseViewModel()
    {
      //  OnPropertyChanged(nameof(IsBusy));
    }

#nullable enable
    public void DeviceDisplay_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        MyMainDisplay = DeviceDisplay.Current.MainDisplayInfo;
        OnPropertyChanged(nameof(MyMainDisplay));
        Orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
    }

#nullable disable
    public static int OnDeviceOrientationChange()
    {
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            return 3;
        else if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
            return 1;
        else if (DeviceInfo.Current.Idiom == DeviceIdiom.Tablet && DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
            return 2;
        else if (DeviceInfo.Current.Idiom == DeviceIdiom.Tablet && DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Landscape) { return 3; }
        else return 1;
    }
}
