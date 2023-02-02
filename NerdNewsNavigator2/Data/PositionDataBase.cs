// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Data;

public class PositionDataBase
{
    readonly string _dbPath;
    private SQLiteConnection _connection;
    public PositionDataBase(string dbPath)
    {
        _dbPath = dbPath;
    }
    public void Init()
    {
        _connection = new SQLiteConnection(_dbPath);
        _connection.CreateTable<Position>();
    }
    public void DeleteAll()
    {
        _connection.DeleteAll<Position>();
    }
    public List<Position> GetAllPositions()
    {
        Init();
        return _connection.Table<Position>().ToList();
    }
    public void Add(Position position)
    {
        _connection = new SQLiteConnection(_dbPath);
        _connection.Insert(position);
    }
    public void Delete(Position position)
    {
        _connection = new SQLiteConnection(_dbPath);
        _connection.Delete(position);
    }
}
