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
    public List<Position> GetCurrentPosition()
    {
        return Current;
    }
    public async Task<bool> SaveCurrentPosition(Position position)
    {
        var temp = App.PositionData.GetAllPositions().Result;
        foreach (var item in temp)
        {
            if (item.Title == position.Title)
            {
                await App.PositionData.Delete(position);
            }
        }
        await App.PositionData.Add(position);
        if (Current.Contains(position))
        {
            Current.Remove(position);
        }
        Current.Add(position);

        return true;
    }
}
