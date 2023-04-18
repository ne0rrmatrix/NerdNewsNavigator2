// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;

public static class FavoriteService
{
    /// <summary>
    /// A <see cref="bool"/> instance managed by this Class.
    /// </summary>
    public static bool IsDownloading { get; set; } = false;

    /// <summary>
    /// Method Adds Favorites <see cref="Show"/> to Database.
    /// </summary>
    /// <param name="favorites">Is the Url of <see cref="Show.Url"/> to Add to datbase.</param> 
    /// <returns>nothing</returns>
    public static async Task<bool> AddFavoriteToDatabase(Show favorites)
    {
        var items = await App.PositionData.GetAllFavorites();
        if (items.AsEnumerable().Any(x => x.Url == favorites.Url))
        {
            return false;
        }
        await App.PositionData.AddFavorites(favorites);
        return true;
    }

    /// <summary>
    /// Method Removes Favorites <see cref="Show"/> from Database.
    /// </summary>
    /// <param name="url"> is the Favorite to remove from database.</param>
    /// <returns>nothing</returns>
    public static async Task<bool> RemoveFavoriteFromDatabase(string url)
    {
        var temp = await App.PositionData.GetAllFavorites();
        var result = temp.AsEnumerable().First(temp => temp.Url == url);
        if (result != null)
        {
            await App.PositionData.DeleteFavorite(result);
            return true;
        }
        return false;
    }
}
