// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2;
public class RetryHandler : DelegatingHandler
{
    // Strongly consider limiting the number of retries - "retry forever" is
    // probably not the most user friendly way you could respond to "the
    // network cable got pulled out."
    private const int MaxRetries = 3;

    public RetryHandler(HttpMessageHandler innerHandler)
        : base(innerHandler)
    { }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = null;
        for (int i = 0; i < MaxRetries; i++)
        {
            response = await base.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }
        }

        return response;
    }
}
