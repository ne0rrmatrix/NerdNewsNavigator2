// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;
public class PositionServices
{
    public List<Position> Current { get; set; } = new();
    public PositionServices()
    {
        _ = Data();
    }
    public async Task Data()
    {
        Current = await App.PositionData.GetAllPositions();
    }
    public async Task DeleteAll()
    {
        await App.PositionData.DeleteAll();
    }
    public Task<List<Position>> GetAllPositions()
    {
        return Task.FromResult(Current);
    }
    public async Task<bool> SaveCurrentPosition(Position position)
    {
        //var temp = App.PositionData.GetAllPositions().Result;
        foreach (var item in Current.ToList())
        {
            Debug.WriteLine($"going through list: {item.Title} at: {item.SavedPosition.TotalSeconds}");
            if (item.Title == position.Title)
            {
                Debug.WriteLine($"Found: {item.Title} at: {item.SavedPosition.TotalSeconds}");
                await App.PositionData.Delete(item);
                Current.Remove(item);
            }
        }
        await App.PositionData.Add(position);
        Current.Add(position);

        return true;
    }
}
