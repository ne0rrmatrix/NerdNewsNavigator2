﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.View;

/// <summary>
/// A class that Manages displaying Podcasts from <see cref="PodcastServices"/>
/// </summary>
public partial class SettingsPage : ContentPage
{
    private readonly IMessenger _messenger;
    /// <summary>
    /// an <see cref="int"/> instance managed by this class.
    /// </summary>
    private string _setAutoDownload;

    /// <summary>
    /// an <see cref="int"/> instance managed by this class.
    /// </summary>
    public string SetAutoDownload
    {
        get { return _setAutoDownload; }
        set
        {
            _setAutoDownload = value;
            OnPropertyChanged(nameof(_setAutoDownload));
        }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SettingsPage"/>
    /// </summary>
    /// <param name="viewModel">The <see cref="ViewModel"/> instance that is managed through this class.</param> 
    public SettingsPage(SettingsViewModel viewModel, IMessenger messenger)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _messenger = messenger;
    }

    #region Buttons
    /// <summary>
    /// The Method controls Adding a <see cref="Podcast"/> to <see cref="List{T}"/> of class <see cref="Podcast"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddPodcast(object sender, EventArgs e)
    {
        if (sender is null || Url.Text is null)
        {
            return;
        }
        if (ValidateUrl(Url.Text) && Uri.IsWellFormedUriString(Url.Text, UriKind.Absolute) && Url.Text.Contains("twit"))
        {
            await PodcastServices.AddPodcast(Url.Text.ToString());
            await Toast.Make("Podcast Added!.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            await Shell.Current.GoToAsync($"{nameof(SettingsPage)}");
            return;
        }
        await Toast.Make("Error: Not a twit Podcast!", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
    }
    private void AutoDownload(object sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        if (SetAutoDownload == "Yes")
        {
            SetAutoDownload = "No";
            OnPropertyChanged(nameof(SetAutoDownload));
            //Preferences.Default.Remove("SetAutoDownload");
            _ = Task.Run(() =>
            {
                Preferences.Default.Remove("start");
                Thread.Sleep(1000);
                Preferences.Default.Set("start", false);
                _messenger.Send(new MessageData(false));
            });
#if ANDROID
            //MainActivity.SetAutoDownload = false;
#endif
        }
        else
        {
            SetAutoDownload = "Yes";
            OnPropertyChanged(nameof(SetAutoDownload));
            //Preferences.Default.Remove("SetAutoDownload");
            _ = Task.Run(() =>
            {
                Preferences.Remove("start");
                Thread.Sleep(1000);
                Preferences.Default.Set("start", true);
                _messenger.Send(new MessageData(true));
            });
#if ANDROID
            //MainActivity.SetAutoDownload = true;
#endif
        }
    }
    /// <summary>
    /// Method validates URL <see cref="Uri"/> <see cref="string"/>
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private static bool ValidateUrl(string url)
    {
        if (url.Trim() == string.Empty)
        {
            return false;
        }
        var pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
        var rgx = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
        return rgx.IsMatch(url);
    }

    /// <summary>
    /// The Method controls adding all default <see cref="Podcast"/> to <see cref="List{T}"/> of class <see cref="Podcast"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddDefault(object sender, EventArgs e)
    {
        await PodcastServices.AddDefaultPodcasts();
        await Toast.Make("Defaults Added!.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
    }

    /// <summary>
    /// The Method controls Removing Default <see cref="Podcast"/> from <see cref="List{T}"/> of class <see cref="Podcast"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveDefault(object sender, EventArgs e)
    {
        var item = await App.PositionData.GetAllPodcasts();
        if (item.AsEnumerable().Any(x => !x.Url.Contains("feeds.twit.tv")))
        {
            await PodcastServices.RemoveDefaultPodcasts();
            await Toast.Make("Defaults removed.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        }
        else
        {
            await DisplayAlert("", "At least one podcast needs to be added", "Ok");
        }
    }

    /// <summary>
    /// The Method manages Removing a <see cref="Podcast"/> from <see cref="List{T}"/> of class <see cref="Podcast"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void EditPodcasts(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(EditPage)}");
    }

    /// <summary>
    /// The Method resets <see cref="List{T}"/> of class <see cref="Podcast"/> to defaults.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>

    private async void ResetPodcasts(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(ResetAllSettingsPage)}");
    }

    /// <summary>
    /// The Method opens donation url in default browser
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OpenUrlForDonation(object sender, EventArgs e)
    {
        var item = "https://www.paypal.com/donate/?business=LYEHGH249KCP2&no_recurring=0&item_name=All+donations+are+welcome.+It+helps+support+development+of+NerdNewsNavigator.+Thank+you+for+your+support.&currency_code=CAD";
        await Browser.OpenAsync(item);
    }
    #endregion

    /// <summary>
    /// Method sets screen to normal screen size.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        var start = Preferences.Default.Get("start", true);
        if (start)
        {
            SetAutoDownload = "Yes";
        }
        else
        {
            SetAutoDownload = "No";
        }
        OnPropertyChanged(nameof(SetAutoDownload));
        DeviceService.RestoreScreen();
        if (DownloadService.IsDownloading)
        {
            Shell.SetNavBarIsVisible(Shell.Current.CurrentPage, true);
        }
    }
}
