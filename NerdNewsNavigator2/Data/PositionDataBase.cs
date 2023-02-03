// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Data;

public class PositionDataBase
{
    private SQLiteAsyncConnection _connection;
    public PositionDataBase()
    {
    }
    public async Task Init()
    {
        var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyData.db");
        _connection = new SQLiteAsyncConnection(databasePath);
        await _connection.CreateTableAsync<Position>();
    }
    public async Task DeleteAll()
    {
        try
        {
            await _connection.DeleteAllAsync<Position>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Cannot delete data!. " + ex.Message);
        }
    }
    public async Task<List<Position>> GetAllPositions()
    {
        await Init();
        var test = await _connection.Table<Position>().ToListAsync();
        return test;
    }
    public async Task Add(Position position)
    {
        try
        {
            await _connection.InsertAsync(position);
        }
        catch
        {
        }
    }
    public async Task Delete(Position position)
    {
        try
        {
            await _connection.DeleteAsync(position);
        }
        catch { }
    }
}
