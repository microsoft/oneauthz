// -------------------------------------------------------------------------------
// <copyright file="TestRunner.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------

namespace Functions.Runner
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Invokes sample HTTP Trigger function
    /// </summary>
    public class TestRunner
    {
        /// <summary>
        /// Holds onto ILogger instance if one is provided
        /// </summary>
        private readonly ILogger logger = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunner" />
        /// </summary>
        /// <param name="l">parameter must not be empty</param>
        public TestRunner(ILogger l)
        {
            logger = l;
            logger?.LogInformation("Entered the TestRunner object.");
        }

        /// <summary>
        /// Primary function
        /// </summary>
        /// <param name="options">parameter must not be empty, provides AAD needed parameters</param>
        /// <param name="endpoint">parameter must not be empty</param>
        public void InvokeHttpTrigger(AzureAdOptions options, string endpoint)
        {
            logger?.LogInformation($"Invoking GET on {endpoint}.");

            using (var client = new HttpClient())
            {
                var token = TokenGetter.GetAppTokenAsync(options, Constants.FunctionsResource).GetAwaiter();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.AuthenticationHeader, token.GetResult());

                var url = QueryHelpers.AddQueryString(endpoint, Constants.QueryStringParameter, Constants.QueryStringValue);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url)
                };

                var response = client.SendAsync(request).GetAwaiter();
                var result = response.GetResult();
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    logger?.LogInformation(result.Content.ReadAsStringAsync().GetAwaiter().GetResult().ToString());
                }
                else
                {
                    logger?.LogError($"Call to {endpoint} failed. Received {result.StatusCode.ToString()}.");
                }
            }
        }
    }
}
