// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Data;

/// <summary>
/// A class for managing the Database.
/// </summary>
public class PositionDataBase
{
    #region Properties
    /// <summary>
    /// A variable to manage logs.
    /// </summary>
    private readonly ILogger _logger = LoggerFactory.GetLogger(nameof(PositionDataBase));

    /// <summary>
    /// A variable to manage <see cref="SQLiteAsyncConnection"/>
    /// </summary>
    private readonly SQLiteAsyncConnection _connection;

    public const SQLite.SQLiteOpenFlags Flags = SQLite.SQLiteOpenFlags.ReadWrite | SQLite.SQLiteOpenFlags.Create | SQLite.SQLiteOpenFlags.SharedCache;

    #endregion
    /// <summary>
    /// Intializes a new instance of the <see cref="PositionDataBase"/> class.
    /// </summary>
    public PositionDataBase()
    {
        try
        {
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyData.db");
            _logger.Info($"Database path is: {databasePath}");
            _connection = new SQLiteAsyncConnection(databasePath, Flags);

            _connection.CreateTableAsync<Position>();
            _logger.Info($"Position Init {databasePath}");
            _connection.CreateTableAsync<Podcast>();
            _logger.Info($"Podcast Init {databasePath}");
            _connection.CreateTableAsync<Download>();
            _logger.Info($"Download Init {databasePath}");
            _connection.CreateTableAsync<Favorites>();
            _logger.Info($"Favorites Init {databasePath}");
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to start Database: {Message}", ex.Message);
        }
    }

    #region Get Data
    /// <summary>
    /// Method retrieves a <see cref="List{T}"/> of <see cref="Position"/> from database.
    /// </summary>
    /// <returns><see cref="List{T}"/> <see cref="Position"/></returns>
    public async Task<List<Position>> GetAllPositions()
    {
        var test = await _connection.Table<Position>().ToListAsync();
        if (test is null)
        {
            _logger.Info("Returning empty collection");
            return new List<Position>();
        }
        _logger.Info("Got all Positions from Database.");
        return test;
    }

    /// <summary>
    /// Method Retrieves a <see cref="List{T}"/> of <see cref="Podcast"/> from database.
    /// </summary>
    /// <returns><see cref="List{T}"/> <see cref="Podcast"/></returns>
    public async Task<List<Podcast>> GetAllPodcasts()
    {
        var temp = await _connection.Table<Podcast>().ToListAsync();
        if (temp is null)
        {
            _logger.Info("Returning empty collection");
            return new List<Podcast>();
        }
        _logger.Info("Got all Podcasts from Database.");
        return temp;
    }

    /// <summary>
    /// Method Retrieves a <see cref="List{T}"/> of <see cref="Favorites"/> from database.
    /// </summary>
    /// <returns><see cref="List{T}"/> <see cref="Favorites"/></returns>
    public async Task<List<Favorites>> GetAllFavorites()
    {
        var temp = await _connection.Table<Favorites>().ToListAsync();
        if (temp is null)
        {
            _logger.Info("Returning empty collection");
            return new List<Favorites>();
        }
        _logger.Info("Got all Favorites from Database.");
        return temp;
    }

    /// <summary>
    /// Method Retrieves a <see cref="List{T}"/> of <see cref="Download"/> from database.
    /// </summary>
    /// <returns><see cref="List{T}"/> <see cref="Download"/></returns>
    public async Task<List<Download>> GetAllDownloads()
    {
        var temp = await _connection.Table<Download>().ToListAsync();
        if (temp is null)
        {
            _logger.Info("Returning empty collection");
            return new List<Download>();
        }
        _logger.Info("Got all Downloads from Database.");
        return temp;
    }

    #endregion

    #region Delete by Table

    /// <summary>
    /// Method Deletes all <see cref="Position"/> from database.
    /// </summary>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> DeleteAllPositions()
    {
        try
        {
            await _connection.DeleteAllAsync<Position>();
            _logger.Info("Deleted All Position data.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to delete Position data from database: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Method deletes all <see cref="Podcast"/> from database.
    /// </summary>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> DeleteAllPodcasts()
    {
        try
        {

            await _connection.DeleteAllAsync<Podcast>();
            _logger.Info("Succesfully Deleted all Podcasts.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to delete all Podcasts. {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Method deletes all <see cref="Favorites"/> from database.
    /// </summary>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> DeleteAllFavorites()
    {
        try
        {

            await _connection.DeleteAllAsync<Favorites>();
            _logger.Info("Succesfully Deleted all Favorites.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to delete all Favorites. {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Method deletes all <see cref="Download"/> from database.
    /// </summary>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> DeleteAllDownloads()
    {
        try
        {

            await _connection.DeleteAllAsync<Download>();
            _logger.Info("Succesfully Deleted all Downloads.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to delete all Downloads. {ex.Message}");
            return false;
        }
    }
    #endregion

    #region Add Data
    /// <summary>
    /// Method Adds a <see cref="Position"/> to Database
    /// </summary>
    /// <param name="position">An instance of <see cref="Position"/></param>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> AddPosition(Position position)
    {
        try
        {
            await _connection.InsertAsync(position);
            _logger.Info($"Saved to database Position: {position.Title} {position.SavedPosition}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to save to Database Position: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Method add a <see cref="Podcast"/> to Database.
    /// </summary>
    /// <param name="podcast">an instance of <see cref="Podcast"/></param>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> AddPodcast(Podcast podcast)
    {
        try
        {
            await _connection.InsertAsync(podcast);
            _logger.Info($"Saved to Database Podcast: {podcast.Title}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Info($"$Failed to save Podcast to Database: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Method add a <see cref="Download"/> to Database.
    /// </summary>
    /// <param name="download">an instance of <see cref="Download"/></param>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> AddDownload(Download download)
    {
        try
        {
            await _connection.InsertAsync(download);
            _logger.Info($"Saved to Database Download: {download.Title}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Info($"Failed to save Download to Database: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Method add a <see cref="Favorites"/> to Database.
    /// </summary>
    /// <param name="favorites">an instance of <see cref="Favorites"/></param>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> AddFavorites(Favorites favorites)
    {
        try
        {
            await _connection.InsertAsync(favorites);
            _logger.Info($"Saved to Database Favorites: {favorites.Title}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to save Favorites to Database: {ex.Message}");
            return false;
        }
    }
    #endregion

    #region Update Data

    /// <summary>
    /// Method Updates a <see cref="Position"/> from Database
    /// </summary>
    /// <param name="position">an instance of <see cref="Position"/></param>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> UpdatePosition(Position position)
    {
        try
        {
            var existingPositon = await _connection.Table<Position>().Where(x => x.Title == position.Title).FirstOrDefaultAsync();
            if (existingPositon is not null)
            {
                existingPositon.SavedPosition = position.SavedPosition;
                await _connection.UpdateAsync(existingPositon);
                _logger.Info($"Updated Database: {existingPositon.Title} {existingPositon.SavedPosition}");
                return true;
            }
            await _connection.InsertAsync(position);
            _logger.Info($"Adding to Database: {position.Title} {position.SavedPosition}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to Update Position from Database {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Method Update a <see cref="Podcast"/> from Database
    /// </summary>
    /// <param name="podcast">an instance of <see cref="Podcast"/></param>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> UpdatePodcast(Podcast podcast)
    {
        try
        {
            var existingPositon = await _connection.Table<Podcast>().Where(x => x.Title == podcast.Title).FirstOrDefaultAsync();
            if (existingPositon is not null)
            {
                existingPositon.Download = podcast.Download;
                existingPositon.IsNotDownloaded = podcast.IsNotDownloaded;
                await _connection.UpdateAsync(existingPositon);
                _logger.Info($"Updated Podcast: {existingPositon.Title}");
                return true;
            }
            await _connection.InsertAsync(podcast);
            _logger.Info($"Added Podcast: {podcast.Title}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to Update Podcast from Database: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Method Updates a <see cref="Download"/> from Database
    /// </summary>
    /// <param name="download">an instance of <see cref="Download"/></param>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> UpdateDownload(Download download)
    {
        try
        {
            var existingPositon = await _connection.Table<Download>().Where(x => x.Title == download.Title).FirstOrDefaultAsync();
            if (existingPositon is not null)
            {
                // don't know why I need to do it this way!
                existingPositon.Deleted = download.Deleted;
                await _connection.UpdateAsync(existingPositon);
                _logger.Info($"Updated Download: {existingPositon.Title}");
                return true;
            }
            await _connection.InsertAsync(download);
            _logger.Info($"Added Download: {download.Title}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to Download favorite from Database: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Delete Individual Items
    /// <summary>
    /// Method deletes a <see cref="Position"/> from Database
    /// </summary>
    /// <param name="position">an instance of <see cref="Position"/></param>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> Delete(Position position)
    {
        try
        {
            await _connection.DeleteAsync(position);
            _logger.Info($"Deleted from Database: {position.Title} {position.SavedPosition}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to Delete Position from Database {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Method Deletes a <see cref="Podcast"/> from Database
    /// </summary>
    /// <param name="podcast">an instance of <see cref="Podcast"/></param>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> DeletePodcast(Podcast podcast)
    {
        try
        {
            await _connection.DeleteAsync(podcast);
            _logger.Info($"Deleted Podcast: {podcast.Title}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to Delete Podcast from Database: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Method Deletes a <see cref="Favorites"/> from Database
    /// </summary>
    /// <param name="favorites">an instance of <see cref="Favorites"/></param>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> DeleteFavorite(Favorites favorites)
    {
        try
        {
            await _connection.DeleteAsync(favorites);
            _logger.Info($"Deleted Favorite: {favorites.Title}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to Delete favorite from Database: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Method Deletes a <see cref="Download"/> from Database
    /// </summary>
    /// <param name="download">an instance of <see cref="Download"/></param>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> DeleteDownload(Download download)
    {
        try
        {
            await _connection.DeleteAsync(download);
            _logger.Info($"Deleted Podcast: {download.Title}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to Delete Download from Database: {ex.Message}");
            return false;
        }
    }

    #endregion
}
