// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;

public class PositionServices
{
    public Position Position { get; set; } = new();
    public PositionServices()
    {
    }

    public Position GetCurrentPosition()
    {
        return Position;
    }
    public Task SaveCurrentPosition(Position position)
    {
        Position = position;
        return Task.CompletedTask;
    }
}
