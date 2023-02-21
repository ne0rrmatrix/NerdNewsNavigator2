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
        var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyData.db");
        _connection = new SQLiteAsyncConnection(databasePath);
        await _connection.CreateTableAsync<Position>();
        return;
    }
    public async Task PodcastInit()
    {
        var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyDataPodcast.db");
        _podcastConnection = new SQLiteAsyncConnection(databasePath);
        await _podcastConnection.CreateTableAsync<Podcast>();
        return;
    }
    public async Task DeleteAll()
    {
        await _connection.DeleteAllAsync<Position>();
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
        if (_podcastConnection != null)
            await _podcastConnection.DeleteAllAsync<Podcast>();
    }
    public async Task<bool> Add(Position position)
    {
        var test = await GetAllPositions();

        foreach (var item in test)
        {
            if (item.Title == position.Title)
            {
                await _connection.DeleteAsync(item);
            }
        }

        await _connection.InsertAsync(position);
        return true;
    }
    public async Task Delete(Position position)
    {
        await _connection.DeleteAsync(position);
    }
    public async Task AddPodcast(Podcast podcast)
    {
        await _podcastConnection.InsertAsync(podcast);
    }
    public async Task DeletePodcast(Podcast podcast)
    {
        await _podcastConnection.DeleteAsync(podcast);
    }
}
