// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdNewsNavigator2.Service;

public static class ViewService
{
    private static IServiceCollection Services;

    public static MauiAppBuilder UseViewServices(this MauiAppBuilder builder)
    {
        Services = builder.Services;
        return builder;
    }

    public static ContentPage ResolvePage<TService>(params object[] inputParameters)
    {
        var implementationType = Services
                                .Where(service => service.ServiceType == typeof(TService) && service.ImplementationType != null)
                                .Select(service => service.ImplementationType)
                                .First();

        return (Activator.CreateInstance(implementationType, inputParameters)) as ContentPage;
    }
}
