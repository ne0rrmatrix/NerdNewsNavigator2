﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

/// <summary>
/// <c>BaseViewModel</c> is a <see cref="ViewModel"/> class that can be Inherited.
/// </summary>
public partial class BaseViewModel : ObservableObject, IRecipient<InternetItemMessage>, IRecipient<DownloadItemMessage>
{
    #region Properties
    public delegate void DownloadChangedHandler();

    public event DownloadChangedHandler DownloadChanged;
    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of <see cref="Show"/> managed by this class.
    /// </summary>
    public ObservableCollection<Show> FavoriteShows { get; set; } = new();
    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of <see cref="Show"/> managed by this class.
    /// </summary>
    public ObservableCollection<Show> Shows { get; set; } = new();

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of most recent <see cref="Show"/> managed by this class.
    /// </summary>
    public ObservableCollection<Show> MostRecentShows { get; set; } = new();

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of downloaded <see cref="Download"/> managed by this class.
    /// </summary>
    public ObservableCollection<Download> DownloadedShows { get; set; } = new();

    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> of <see cref="Podcast"/> managed by this class.
    /// </summary>
    public ObservableCollection<Podcast> Podcasts { get; set; } = new();

    /// <summary>
    /// The <see cref="bool"/> instance managed by this class.
    /// </summary>

    /// <summary>
    /// The <see cref="DisplayInfo"/> instance managed by this class.
    /// </summary>
    public DisplayInfo MyMainDisplay { get; set; } = new();

    /// <summary>
    /// An <see cref="ILogger{TCategoryName}"/> instance managed by this class.
    /// </summary>
    ILogger<BaseViewModel> Logger { get; set; }

    /// <summary>
    /// an <see cref="IConnectivity"/> instance managed by this class.
    /// </summary>
    private readonly IConnectivity _connectivity;

    /// <summary>
    /// an <see cref="int"/> instance managed by this class. Used to set <see cref="Span"/> of <see cref="GridItemsLayout"/>
    /// </summary>
    private int _orientation;

    /// <summary>
    /// an <see cref="int"/> instance managed by this class.
    /// </summary>
    private bool _isDownloading;

    /// <summary>
    /// an <see cref="int"/> instance managed by this class.
    /// </summary>
    public bool IsDownloading
    {
        get => _isDownloading;
        set
        {
            if (SetProperty(ref _isDownloading, value))
            {
                OnPropertyChanged(nameof(IsNotDownloading));
            }

        }
    }
    public bool IsNotDownloading => !IsDownloading;

    /// <summary>
    /// An <see cref="int"/> public property managed by this class. Used to set <see cref="Span"/> of <see cref="GridItemsLayout"/>
    /// </summary>
    public int Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }

    /// <summary>
    /// an <see cref="int"/> instance managed by this class.
    /// </summary>
    private string _downloadProgress;

    private string _title;
    public string Title
    {
        get { return _title; }
        set
        {
            _title = value;
            OnPropertyChanged(nameof(Title));
        }
    }

    /// <summary>
    /// an <see cref="int"/> instance managed by this class.
    /// </summary>
    public string DownloadProgress
    {
        get => _downloadProgress;
        set => SetProperty(ref _downloadProgress, value);
    }

    /// <summary>
    /// A <see cref="bool"/> instance managed by this class. 
    /// </summary>
    private bool _isBusy;

    /// <summary>
    /// A <see cref="bool"/> public property managed by this class.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }
    }

    /// <summary>
    /// A <see cref="bool"/> public property managed by this class.
    /// </summary>
    public bool IsNotBusy => !IsBusy;

    #endregion
    public BaseViewModel(ILogger<BaseViewModel> logger, IConnectivity connectivity)
    {
        Logger = logger;
        this._connectivity = connectivity;
        _downloadProgress = string.Empty;
        DownloadChanged += () =>
        {
            Logger.LogInformation("NavBar closed");
        };
        WeakReferenceMessenger.Default.Reset();
        WeakReferenceMessenger.Default.Register<DownloadItemMessage>(this);
        WeakReferenceMessenger.Default.Register<InternetItemMessage>(this);
        ThreadPool.QueueUserWorkItem(GetDownloadedShows);
        ThreadPool.QueueUserWorkItem(GetFavoriteShows);
    }

    #region Messaging Service

    /// <summary>
    /// Method invokes <see cref="MessagingService.RecievedDownloadMessage(bool,string)"/> for displaying <see cref="Toast"/>
    /// </summary>
    /// <param name="message"></param>
    public void Receive(DownloadItemMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await MessagingService.RecievedDownloadMessage(message.Value, message.Title);
        });
    }

    /// <summary>
    /// Method invokes <see cref="MessagingService.RecievedInternetMessage(bool)"/> for displaying <see cref="Toast"/>
    /// </summary>
    /// <param name="message"></param>
    public void Receive(InternetItemMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await MessagingService.RecievedInternetMessage(message.Value);
        });
    }
    #endregion
    /// <summary>
    /// A method that checks if the internet is connected and returns a <see cref="bool"/> as answer.
    /// </summary>
    /// <returns></returns>
    public bool InternetConnected()
    {
        if (_connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #region Download Tasks
    private void TriggerProgressChanged()
    {
        DownloadChanged();
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            IsDownloading = false;
            DownloadService.IsDownloading = false;
            Shell.SetNavBarIsVisible(Shell.Current.CurrentPage, false);
        });
    }

    /// <summary>
    /// A method that download a show to device.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="mostRecent"></param>
    /// <returns></returns>
    public async Task Downloading(string url, bool mostRecent)
    {
        if (!InternetConnected())
        {
            WeakReferenceMessenger.Default.Send(new InternetItemMessage(false));
            return;
        }
        var downloadTemp = DownloadedShows.Any(x => x.Url == url);
        if (downloadTemp)
        {
            return;
        }
        while (IsDownloading)
        {
            Thread.Sleep(5000);
            Logger.LogInformation("Waiting for download to finish");
        }
        await StartDownload(url, mostRecent);
    }
    public async Task StartDownload(string url, bool mostRecent)
    {
        Shell.SetNavBarIsVisible(Shell.Current.CurrentPage, true);
        IsBusy = true;
        ThreadPool.QueueUserWorkItem(state => { UpdatingDownload(); });
        Logger.LogInformation("Trying to start download of {URL}", url);

        List<Show> list;
        if (mostRecent)
        {
            list = MostRecentShows.ToList();
        }
        else
        {
            list = Shows.ToList();
        }

        var item = list.AsEnumerable().First(x => x.Url == url);
        await ProcessDownloads(item, url);
        TriggerProgressChanged();
    }
    public async Task ProcessDownloads(Show item, string url)
    {
        if (item == null)
        {
            return;
        }
        Logger.LogInformation("Found match!");
        Download download = new()
        {
            Title = item.Title,
            Url = url,
            Image = item.Image,
            PubDate = item.PubDate,
            Description = item.Description,
            FileName = DownloadService.GetFileName(url)
        };
        var downloaded = await DownloadService.DownloadShow(download);
        if (downloaded)
        {
            await DownloadSuccess(download);
        }
    }
    public void UpdatingDownload()
    {
        DownloadService.IsDownloading = true;
        IsDownloading = true;
        while (DownloadService.IsDownloading)
        {
            DownloadProgress = DownloadService.Status;
            Title = DownloadProgress;
            Thread.Sleep(1000);
        }
        TriggerProgressChanged();
    }
    public async Task DownloadSuccess(Download download)
    {
        Logger.LogInformation("Downloaded file: {file}", download.FileName);
        var result = await App.PositionData.GetAllDownloads();
        var item = result.Find(result => result.Title == download.Title);
        if (item != null)
        {
            await App.PositionData.DeleteDownload(item);
        }
        await DownloadService.AddDownloadDatabase(download);
        IsDownloading = false;
        DownloadService.IsDownloading = false;
        WeakReferenceMessenger.Default.Send(new DownloadItemMessage(true, download.Title));
    }
    #endregion

    #region Podcast data functions

    /// <summary>
    /// Method gets the <see cref="ObservableCollection{T}"/> of <see cref="Show"/> from the database.
    /// </summary>
    /// <param name="stateinfo"></param>
    public async void GetFavoriteShows(object stateinfo)
    {
        var shows = new ObservableCollection<Show>();
        var temp = await App.PositionData.GetAllFavorites();
        if (temp is null)
        {
            return;
        }
        temp.ForEach(async item =>
        {
            var show = await FeedService.GetShows(item.Url, true);
            if (show is null || show.Count == 0)
            {
                return;
            }
            shows.Add(show.First());
        });
        FavoriteShows.Clear();
        FavoriteShows = new ObservableCollection<Show>(shows);
        OnPropertyChanged(nameof(FavoriteShows));
    }

    /// <summary>
    /// <c>GetShows</c> is a <see cref="Task"/> that takes a <see cref="string"/> for <see cref="Show.Url"/> and returns a <see cref="Show"/>
    /// </summary>
    /// <param name="url"></param> <see cref="string"/> URL of Twit tv Show
    /// <param name="getFirstOnly"><see cref="bool"/> Get first item only.</param>
    /// <returns><see cref="Show"/></returns>
    public async Task GetShows(string url, bool getFirstOnly)
    {
        Shows.Clear();
        if (InternetConnected())
        {
            var temp = await FeedService.GetShows(url, getFirstOnly);
            Shows = new ObservableCollection<Show>(temp);
            OnPropertyChanged(nameof(Shows));
        }
    }

    /// <summary>
    /// Method gets most recent episode from each podcast on twit.tv
    /// </summary>
    /// <returns></returns>
    public async Task GetMostRecent()
    {
        MostRecentShows.Clear();
        await GetUpdatedPodcasts();
        if (InternetConnected() && Podcasts is not null && Podcasts.Count > 0)
        {
            Podcasts.ToList().ForEach(async show =>
            {
                var item = await FeedService.GetShows(show.Url, true);
                MostRecentShows.Add(item.First());
            });
        }
    }

    /// <summary>
    /// <c>GetUpdatedPodcasts</c> is a <see cref="Task"/> that sets <see cref="Podcasts"/> from either a Database or from the web.
    /// </summary>
    /// <returns></returns>
    public async Task GetUpdatedPodcasts()
    {
        Podcasts.Clear();
        var temp = await App.PositionData.GetAllPodcasts();
        if (InternetConnected() && (temp is null || temp.Count == 0))
        {
            var items = await PodcastServices.GetFromUrl();
            await PodcastServices.AddToDatabase(items);
            items.ForEach(Podcasts.Add);
            return;
        }
        if (temp is not null)
        {
            temp?.ForEach(Podcasts.Add);
        }
    }

    /// <summary>
    /// A method that sets <see cref="DownloadedShows"/> from the database.
    /// </summary>
    /// <param name="stateinfo"></param>
    public async void GetDownloadedShows(object stateinfo)
    {
        DownloadedShows.Clear();
        var temp = await App.PositionData.GetAllDownloads();
        temp?.ForEach(DownloadedShows.Add);
    }

    #endregion

    #region Display Functions

#nullable enable

    /// <summary>
    /// <c>DeviceDisplay_MainDisplayInfoChanged</c> is a method that sets <see cref="Orientation"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void DeviceDisplay_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
#if IOS
        if (sender is null)
        {
            return;
        }
#endif
        MyMainDisplay = DeviceDisplay.Current.MainDisplayInfo;
        OnPropertyChanged(nameof(MyMainDisplay));
        Orientation = OnDeviceOrientationChange();
        OnPropertyChanged(nameof(Orientation));
    }

#nullable disable

    /// <summary>
    /// <c>OnDeviceOrientation</c> is a method that is used to set <see cref="Span"/> of <see cref="GridItemsLayout"/>
    /// </summary>
    /// <returns><see cref="int"/></returns>
    public static int OnDeviceOrientationChange()
    {
        if (DeviceDisplay.Current.MainDisplayInfo.Width < 1920 && DeviceDisplay.Current.MainDisplayInfo.Width != 0 && DeviceInfo.Current.Platform == DevicePlatform.WinUI)
        {
            return 2;
        }
        switch (DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
        {
            case true:
                return DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? 1 : 2;
        }
        switch (DeviceInfo.Current.Idiom == DeviceIdiom.Tablet)
        {
            case true:
                return DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? 2 : 3;
        }
        switch (DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            case true:
                return DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? 2 : 3;
        }
        return 1;
    }
    #endregion
}
