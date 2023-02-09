// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Service;
public class TwitService
{
    public static Task<List<string>> GetListOfPodcasts()
    {
        var fileService = new FileService();
        return Task.FromResult(fileService.GetJsonData());
    }
    public static Task<List<Show>> GetShow(string url)
    {
        return Task.FromResult(FeedService.GetShow(url));
    }
}
