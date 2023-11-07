﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Interfaces;

public interface IFeedService
{
    public List<string> GetPodcastListAsync();
    public Podcast GetFeed(string item);
    public List<Show> GetShows(string items, bool getFirstOnly);
}
