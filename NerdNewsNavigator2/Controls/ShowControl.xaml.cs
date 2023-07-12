// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Controls;

public partial class ShowControl : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Name), typeof(ContentPage), typeof(ShowControl));
    public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(ObservableCollection<Show>), typeof(ShowControl), null);
    public ContentPage Name
    {
        get => GetValue(TitleProperty) as ContentPage;
        set => SetValue(TitleProperty, value);
    }
    public ObservableCollection<Show> Source
    {
        get => GetValue(SourceProperty) as ObservableCollection<Show>;
        set => SetValue(SourceProperty, value);
    }
    public ShowControl()
    {
        InitializeComponent();
    }
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        DeviceService.RestoreScreen();
#if WINDOWS || MACCATALYST
        if (DownloadService.IsDownloading)
        {
            Shell.SetNavBarIsVisible(Shell.Current.CurrentPage, true);
        }
#endif
    }
}
