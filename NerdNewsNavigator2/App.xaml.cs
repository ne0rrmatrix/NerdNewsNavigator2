// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2;

/// <summary>
/// A class that acts as a manager for <see cref="Application"/>
/// </summary>
public partial class App : Application
{
    private static bool IsDownloading { get; set; } = false;
    /// <summary>
    /// This applications Dependancy Injection for <see cref="PositionDataBase"/> class.
    /// </summary>
    public static PositionDataBase PositionData { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="positionDataBase"></param>
    public App(PositionDataBase positionDataBase)
    {
        InitializeComponent();
        MainPage = new AppShell();

        // Database Dependancy Injection START
        PositionData = positionDataBase;
        // Database Dependancy Injection END

        LogController.InitializeNavigation(
            page => MainPage!.Navigation.PushModalAsync(page),
            () => MainPage!.Navigation.PopModalAsync());

        ThreadPool.QueueUserWorkItem(AutoDownload);
    }

    /// <summary>
    /// Method Auto downloads <see cref="Show"/> from Database.
    /// </summary>
    /// <param name="stateinfo"></param>
    public static async void AutoDownload(object stateinfo)
    {
        Debug.WriteLine("Trying to start Auto Download");
        var favoriteShows = await PositionData.GetAllFavorites();
        var downloadedShows = await PositionData.GetAllDownloads();

        if (favoriteShows is null || downloadedShows is null)
        {
            return;
        }
        while (IsDownloading)
        {
            Thread.Sleep(5000);
            Debug.WriteLine("Waiting for download to finish");
        }
        favoriteShows.ForEach(async x =>
        {
            if (!x.IsDownloaded)
            {
                var show = await FeedService.GetShows(x.Url, true);
                while (IsDownloading)
                {
                    Thread.Sleep(5000);
                    Debug.WriteLine("Waiting for download to finish");
                }
                if (!downloadedShows.Any(y => y.Url == x.Url))
                {
                    Debug.WriteLine("Downloading ", show.First().Url);
                    IsDownloading = true;
                    var result = await DownloadService.Downloading(show.First());
                    if (result)
                    {
                        x.IsDownloaded = true;
                        await PositionData.UpdateFavorite(x);
                        IsDownloading = false;
                    }
                    if (!result)
                    {
                        IsDownloading = false;
                    }
                }
            }
        });
    }
}

