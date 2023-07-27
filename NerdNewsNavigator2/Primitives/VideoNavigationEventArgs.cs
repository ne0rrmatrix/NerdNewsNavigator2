// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Primitives;

public class VideoNavigationEventArgs : EventArgs
{
    public Show CurrentShow { get; set; }
    public string Title { get; set; }
}
