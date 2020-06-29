// -------------------------------------------------------------------------------
// <copyright file="HttpTrigger.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------

namespace Functions
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Sample class that implements a simple Azure function based on
    /// the 'Http type' of trigger.
    /// </summary>
    public class HttpTrigger
    {
        /// <summary>
        /// Supplied via dependency injection when HttpTrigger is created
        /// </summary>
        private readonly IAuthorizationHelper client;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        public HttpTrigger(IAuthorizationHelper client)
        {
            this.client = client;
        }

        /// <summary>
        /// Example handler for http based requests that needs to check for
        /// allowed access or not.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("HttpTrigger")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function received a request.");

            // Check for required permissions
            var allowed = await client.CheckAccessAysnc(req, "/Function1", "/Invoke").ConfigureAwait(false);

            if (!allowed)
            {
                return (ActionResult)new UnauthorizedResult();
            }

            string name = req.Query["name"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        /// <summary>
        /// Example handler for http based requests that needs to check for
        /// allowed access or not.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("HttpTrigger2")]
        public async Task<IActionResult> Run2(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function received a request.");

            // Check for required permissions
            var allowed = await client.CheckAccessAysnc(req, "/Function2", "/NotEnabled").ConfigureAwait(false);

            if (!allowed)
            {
                return (ActionResult)new UnauthorizedResult();
            }

            string name = req.Query["name"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
