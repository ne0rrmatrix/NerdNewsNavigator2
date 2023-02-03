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
        if (position.Title != string.Empty)
        {
            foreach (var item in Current)
            {
                if (item.Title == position.Title)
                {
                    await App.PositionData.Delete(item);
                }
            }
            await App.PositionData.Add(new Position
            {
                Title = position.Title,
                SavedPosition = position.SavedPosition
            });
            Current.Clear();
            Current = await App.PositionData.GetAllPositions();
        }
        return true;
    }
}
