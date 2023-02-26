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
    private readonly ILogger<PositionDataBase> _logger;

    /// <summary>
    /// A variable to manage <see cref="SQLiteAsyncConnection"/>
    /// </summary>
    private SQLiteAsyncConnection _connection;
    #endregion
    /// <summary>
    /// Intializes a new instance of the <see cref="PositionDataBase"/> class.
    /// </summary>
    /// <param name="logger">This classes <see cref="ILogger{TCategoryName}"/> instance variable</param>
    public PositionDataBase(ILogger<PositionDataBase> logger)
    {
        _logger = logger;
        _ = Init();
    }

    /// <summary>
    /// Method initializes the database using <see cref="_connection"/> to <see cref="SQLiteConnection"/>
    /// </summary>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> Init()
    {
        try
        {
            if (_connection != null)
            {
                return false;
            }
#if WINDOWS
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyData.db");
#endif
#if ANDROID || IOS || MACCATALYST
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyData.db");
#endif
            _logger.LogInformation("Database path is: {Path}", databasePath);
            _connection = new SQLiteAsyncConnection(databasePath);

            await _connection.CreateTableAsync<Position>();
            _logger.LogInformation("Position Init {DatabasePath}", databasePath);
            await _connection.CreateTableAsync<Podcast>();
            _logger.LogInformation("Podcast Init {DataBasePath}", databasePath);
            await _connection.CreateTableAsync<Download>();
            _logger.LogInformation("Download Init {DatabasePath}", databasePath);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to start Position Database: {Message}", ex.Message);
        }
        return true;
    }

    /// <summary>
    /// Method Deletes all <see cref="Position"/> from database.
    /// </summary>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> DeleteAll()
    {
        try
        {
            await _connection.DeleteAllAsync<Position>();
            _logger.LogInformation($"Deleted All Position data.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete Position data from database: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Method retrieves a <see cref="List{T}"/> of <see cref="Position"/> from database.
    /// </summary>
    /// <returns><see cref="List{T}"/> <see cref="Position"/></returns>
    public async Task<List<Position>> GetAllPositions()
    {
        try
        {
            var test = await _connection.Table<Position>().ToListAsync();
            _logger.LogInformation("Got all Positions from Database.");
            return test;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get all Positions from Database: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Method Retrieves a <see cref="List{T}"/> of <see cref="Podcast"/> from database.
    /// </summary>
    /// <returns><see cref="List{T}"/> <see cref="Podcast"/></returns>
    public async Task<List<Podcast>> GetAllPodcasts()
    {
        try
        {
            var temp = await _connection.Table<Podcast>().ToListAsync();
            _logger.LogInformation("Got all Podcasts from Database.");
            return temp;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get Podcasts from Database: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Method Retrieves a <see cref="List{T}"/> of <see cref="Download"/> from database.
    /// </summary>
    /// <returns><see cref="List{T}"/> <see cref="Download"/></returns>
    public async Task<List<Download>> GetAllDownloads()
    {
        try
        {
            var temp = await _connection.Table<Download>().ToListAsync();
            _logger.LogInformation("Got all Downloads from Database.");
            return temp;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get Downloads from Database: {Message}", ex.Message);
            return null;
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
            _logger.LogInformation("Succesfully Deleted all Podcasts.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete all Podcasts. {Message}", ex.Message);
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
            _logger.LogInformation("Succesfully Deleted all Downloads.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete all Downloads. {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Method Adds a <see cref="Position"/> to Database
    /// </summary>
    /// <param name="position">An instance of <see cref="Position"/></param>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> Add(Position position)
    {
        var test = await GetAllPositions();

        foreach (var item in test)
        {
            if (item.Title == position.Title)
            {
                try
                {

                    await _connection.DeleteAsync(item);
                    _logger.LogInformation("Succesfully deleted Position: {Title}", position.Title);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error deleting Position: {Message}", ex.Message);
                    return false;
                }
            }
        }
        try
        {
            await _connection.InsertAsync(position);
            _logger.LogInformation("Saved to database: {Title}", position.Title);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save to Database: {Message}", ex.Message);
            return false;
        }
    }

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
            _logger.LogInformation("Deleted from Database: {Title}", position.Title);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to Delete Position from Database {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Method add a <see cref="Podcast"/> to Database.
    /// </summary>
    /// <param name="podcast">an instance of <see cref="Position"/></param>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> AddPodcast(Podcast podcast)
    {
        try
        {
            await _connection.InsertAsync(podcast);
            _logger.LogInformation("Saved to Database Podcast: {Title}", podcast.Title);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Failed to save Podcast to Database: {Message}", ex.Message);
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
            _logger.LogInformation("Deleted Podcast: {Title}", podcast.Title);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to Delete Podcast from Database: {Message}", ex.Message);
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
            _logger.LogInformation("Saved to Database Download: {Title}", download.Title);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Failed to save Download to Database: {Message}", ex.Message);
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
            _logger.LogInformation("Deleted Podcast: {Title}", download.Title);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to Delete Download from Database: {Message}", ex.Message);
            return false;
        }
    }
}
