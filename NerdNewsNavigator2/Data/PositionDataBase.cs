// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Data;
public class PositionDataBase
{
    private SQLiteAsyncConnection _connection;
    private SQLiteAsyncConnection _PodcastConnection;
    public PositionDataBase()
    {
    }
    public async Task Init()
    {
        if (_connection is not null)
        {
            return;
        }
        var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyData.db");
        _connection = new SQLiteAsyncConnection(databasePath);
        await _connection.CreateTableAsync<Position>();
    }
    public async Task PodcastInit()
    {
        if (_PodcastConnection is not null)
        {
            return;
        }

        var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyDataPodcast.db");
        _PodcastConnection = new SQLiteAsyncConnection(databasePath);
        await _PodcastConnection.CreateTableAsync<Podcast>();
    }
    public async Task DeleteAll()
    {
        try
        {
            if (_connection is not null)
                await _connection.DeleteAllAsync<Position>();
        }
        catch
        {
        }
    }
    public async Task<List<Position>> GetAllPositions()
    {
        await Init();
        var test = await _connection.Table<Position>().ToListAsync();
        return test;
    }
    public async Task<List<Podcast>> GetAllPodcasts()
    {
        await PodcastInit();
        var temp = await _PodcastConnection.Table<Podcast>().ToListAsync();
        return temp;
    }
    public async Task DeleteAllPodcasts()
    {
        try
        {
            if (_PodcastConnection is not null)
                await _PodcastConnection.DeleteAllAsync<Podcast>();
        }
        catch
        {
        }
    }
    public async Task Add(Position position)
    {
        try
        {
            if (_connection is not null)
                await _connection.InsertOrReplaceAsync(position);
        }
        catch
        {
        }
    }
    public async Task Delete(Position position)
    {
        try
        {
            if (_connection is not null)
                await _connection.DeleteAsync(position);
        }
        catch
        {
        }
    }
    public async Task AddPodcast(Podcast podcast)
    {
        try
        {
            if (_PodcastConnection is not null)
                await _PodcastConnection.InsertAsync(podcast);
        }
        catch
        {
        }
    }
    public async Task DeletePodcast(Podcast podcast)
    {
        try
        {
            if (_PodcastConnection is not null)
                await _PodcastConnection.DeleteAsync(podcast);
        }
        catch { }
    }
}
