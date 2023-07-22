// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Model;

public class Message
{
    public Show ShowItem { get; set; }
    public int Id { get; set; }
    public string Url { get; set; }
    public bool Cancel { get; set; }
}
