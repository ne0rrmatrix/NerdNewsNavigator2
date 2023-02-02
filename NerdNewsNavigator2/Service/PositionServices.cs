// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;

public class PositionServices
{
    public List<Position> Current { get; set; } = new();
    public PositionServices()
    {
        Current = App.PositionData.GetAllPositions();
    }
    public void DeleteAll()
    {
        App.PositionData.DeleteAll();
    }
    public List<Position> GetCurrentPosition()
    {
        return Current;
    }
    public Task SaveCurrentPosition(Position position)
    {
        position.Title = Preferences.Default.Get("New_Url", "Unknown");
        foreach (var item in Current)
        {
            if (item.Title == position.Title)
            {
                Current.Remove(item);
            }
        }
        if (position.Title != "Unknown")
        {
            App.PositionData.Add(new Position
            {
                Title = position.Title,
                SavedPosition = position.SavedPosition
            });
        }
        return Task.CompletedTask;
    }
}
