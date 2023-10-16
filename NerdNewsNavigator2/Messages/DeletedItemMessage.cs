﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Messages;

public class DeletedItemMessage : ValueChangedMessage<bool>
{
    public DeletedItemMessage(bool value) : base(value)
    {
    }
}
