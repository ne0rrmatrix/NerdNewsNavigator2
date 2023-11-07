// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Interfaces;

public interface IDownloadService
{
    public List<Show> Shows { get; }
    public void Add(Show show);
    public void CancelAll();
    public Show Cancel(Show show);
    public Task Start(Show item);
    public Task DownloadSucceeded(Show item);
}
