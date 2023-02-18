// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Data;
public class PositionDataBase
{
    private SQLiteAsyncConnection _connection;
    private SQLiteAsyncConnection _podcastConnection;
    public PositionDataBase()
    {
        _ = Init();
        _ = PodcastInit();
    }
    public async Task Init()
    {
        try
        {
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyData.db");
            _connection = new SQLiteAsyncConnection(databasePath);
            await _connection.CreateTableAsync<Position>();
        }
        catch { }
        return;
    }
    public async Task PodcastInit()
    {
        try
        {
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyDataPodcast.db");
            _podcastConnection = new SQLiteAsyncConnection(databasePath);
            await _podcastConnection.CreateTableAsync<Podcast>();
        }
        catch { }
        return;
    }
    public async Task DeleteAll()
    {
        try
        {
            await _connection.DeleteAllAsync<Position>();
        }
        catch
        {
        }
    }
    public async Task<List<Position>> GetAllPositions()
    {
        var test = await _connection.Table<Position>().ToListAsync();
        return test;
    }
    public async Task<List<Podcast>> GetAllPodcasts()
    {
        var temp = await _podcastConnection.Table<Podcast>().ToListAsync();
        return temp;
    }
    public async Task DeleteAllPodcasts()
    {
        try
        {
            if (_podcastConnection != null)
                await _podcastConnection.DeleteAllAsync<Podcast>();
        }
        catch
        {
        }
    }
    public async Task<bool> Add(Position position)
    {
        try
        {
            var test = await GetAllPositions();

            foreach (var item in test)
            {
                if (item.Title == position.Title)
                {
                    //Debug.WriteLine($"Database item: {item.Title} at: {item.SavedPosition.TotalSeconds} is being deleted to: {position.SavedPosition.TotalSeconds}");
                    await _connection.DeleteAsync(item);
                }
            }

            await _connection.InsertAsync(position);
            //Debug.WriteLine($"Adding {position.Title} - {position.SavedPosition.TotalSeconds} to database!");
        }
        catch
        {
            //Debug.WriteLine("Failed to Insert to database!");
        }
        return true;
    }
    public async Task Delete(Position position)
    {
        try
        {
            await _connection.DeleteAsync(position);
        }
        catch
        {
            //Debug.WriteLine("Failed to delete from database");
        }
    }
    public async Task AddPodcast(Podcast podcast)
    {
        try
        {
            await _podcastConnection.InsertAsync(podcast);
        }
        catch
        {
        }
    }
    public async Task DeletePodcast(Podcast podcast)
    {
        try
        {
            await _podcastConnection.DeleteAsync(podcast);
        }
        catch { }
    }
}
