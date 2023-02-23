// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Data;
/// <summary>
/// Class <c>PositionDatabase</c> Database Management Class
/// </summary>
public class PositionDataBase
{
    private readonly ILogger<PositionDataBase> _logger;
    private SQLiteAsyncConnection _connection;
    private SQLiteAsyncConnection _podcastConnection;
    public PositionDataBase(ILogger<PositionDataBase> logger)
    {
        _logger = logger;
        _ = Init();
        _ = PodcastInit();
    }
    /// <summary>
    /// Method <c>Init</c> Dabase Initialization for Playback Tracking of Podcasts.
    /// </summary>
    /// <returns><c>bool</c></returns>
    public async Task<bool> Init()
    {
        try
        {
            if (_connection != null)
            {
                return false;
            }
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyData.db");
            _connection = new SQLiteAsyncConnection(databasePath);
            await _connection.CreateTableAsync<Position>();
            _logger.LogInformation("Position Init {DatabasePath}", databasePath);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to start Position Database: {Message}", ex.Message);
        }
        return true;
    }
    /// <summary>
    /// Method <c>PodcastInit</c> Database Initialization for Podcast storage.
    /// </summary>
    /// <returns><c>bool</c></returns>
    public async Task<bool> PodcastInit()
    {
        try
        {
            if (_podcastConnection != null)
            {
                return false;
            }
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyDataPodcast.db");
            _podcastConnection = new SQLiteAsyncConnection(databasePath);
            await _podcastConnection.CreateTableAsync<Podcast>();
            _logger.LogInformation("Podcast Init {DatabasePath}", databasePath);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to start Podcast Database: {Message}", ex.Message);
        }
        return true;
    }
    /// <summary>
    /// Method <c>DeleteAll</c> Deletes all Positions from database.
    /// </summary>
    /// <returns><c>bool</c></returns>
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
    /// Method <c>GetAllPositions</c> Get All positions from database.
    /// </summary>
    /// <returns><c>List</c> of <c>Class</c> <c>Position</c></returns>
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
    /// Method <c>GetAllPodcasts</c> Retrieves list of all Podcast from Database.
    /// </summary>
    /// <returns><c>List</c> of <c>Class</c> <c>Podcast</c></returns>
    public async Task<List<Podcast>> GetAllPodcasts()
    {
        try
        {
            var temp = await _podcastConnection.Table<Podcast>().ToListAsync();
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
    /// Method <c>DeleteAllPodcasts</c> Removes all Podcasts from Database.
    /// </summary>
    /// <returns><c>bool</c></returns>
    public async Task<bool> DeleteAllPodcasts()
    {
        try
        {

            await _podcastConnection.DeleteAllAsync<Podcast>();
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
    /// Method <c>Add</c> Adds <see cref="Position"/> <see cref="object"/> to Database
    /// </summary>
    /// <param name="position"></param>
    /// <returns><c>bool</c></returns>
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
    /// Method <c>Delete</c> Removes <see cref="Position"/> <see cref="object"/> from Database
    /// </summary>
    /// <param name="position"></param>
    /// <returns><c>bool</c></returns>
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
    /// Method <c>AddPodcast</c> Adds <see cref="Podcast"/> <see cref="object"/> to Database.
    /// </summary>
    /// <param name="podcast"></param> <see cref="Podcast"/>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> AddPodcast(Podcast podcast)
    {
        try
        {
            await PodcastInit();
            await _podcastConnection.InsertAsync(podcast);
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
    /// Method <c>DeletePodcast</c> Deletes <see cref="Podcast"/> <see cref="object"/> from Database
    /// </summary>
    /// <param name="podcast"></param> <see cref="Podcast"/>
    /// <returns><see cref="bool"/></returns>
    public async Task<bool> DeletePodcast(Podcast podcast)
    {
        try
        {
            await _podcastConnection.DeleteAsync(podcast);
            _logger.LogInformation("Deleted Podcast: {Title}", podcast.Title);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to Delete Podcast from Database: {Message}", ex.Message);
            return false;
        }
    }
}
