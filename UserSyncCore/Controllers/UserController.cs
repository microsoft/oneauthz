// -------------------------------------------------------------------------------
// <copyright file="UserController.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------

namespace UserSyncCore.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.AzureAD.UI;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Enterprise.Authorization.Client.Middleware;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using UserSyncCore;
    using UserSyncCore.Models;

    /// <summary>
    /// Controller for retrieving a list of users from the user's directory
    /// from MS Graph. A common header provides navigation to other pages.
    /// User is authenticated by middleware before controller actions are invoked.
    /// </summary>
    [Authorize]
    public class UserController : Controller
    {
        /// <summary>
        /// TenantIdClaimType
        /// </summary>
        private const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";

        /// <summary>
        /// AuthorityFormat
        /// </summary>
        private const string AuthorityFormat = "https://login.microsoftonline.com/{0}";

        /// <summary>
        /// MSGraphScope
        /// </summary>
        private const string MSGraphScope = "https://graph.microsoft.com/";

        /// <summary>
        /// azureADOptions
        /// </summary>
        private readonly AzureADOptions azureADOptions;

        /// <summary>
        /// Logger is created at app startup and passed in by DI.
        /// </summary>
        private readonly ILogger<UserController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="optionsAccessor">IOptions</param>
        public UserController(ILogger<UserController> logger, IOptions<AzureADOptions> optionsAccessor)
        {
            this.logger = logger;
            this.azureADOptions = optionsAccessor.Value;
        }

        /// <summary>
        /// Action for GET of user page listing users.
        /// </summary>
        /// <returns>Task<ActionResult></returns>
        [AadCheckAccessAuthorize("/Users", "/Read")]
        public async Task<ActionResult> Index()
        {
            string tenantId = string.Empty;
            List<User> users;

            try
            {
                tenantId = this.HttpContext.User.Claims.FirstOrDefault(x => x.Type == TenantIdClaimType)?.Value ?? "Unknown";
                this.ViewBag.TenantId = tenantId;
                users = await this.GetUsersAsync(tenantId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.logger.LogError(string.Format(CultureInfo.InvariantCulture, "Failed to get list of users from directory: {0}", e.Message));
                return this.Redirect("/Home/Error");
            }

            return this.View(new UserViewModel(users));
        }

        /// <summary>
        /// Gets a list of users from the tenant. For demonstration purposes,
        /// the list of users is limited to 20 via OData filter.
        /// </summary>
        /// <param name="tenantId">string</param>
        /// <returns>Task<List<User>></returns>
        private async Task<List<User>> GetUsersAsync(string tenantId)
        {
            var graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
            {
                string idToken = await this.HttpContext.GetTokenAsync("id_token").ConfigureAwait(false);

                var tg = new TokenGetter(
                    idToken,
                    string.Format(CultureInfo.InvariantCulture, AuthorityFormat, tenantId),
                    this.azureADOptions.ClientId,
                    this.azureADOptions.ClientSecret);

                var token = await tg.AcquireTokenOnBehalfOfAsync(MSGraphScope, this.HttpContext.User).ConfigureAwait(false);

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                requestMessage.Headers.Add("Accept-Encoding", "*");
                requestMessage.Headers.Add("Accept", "text/*, application/*");
            }));

            List<QueryOption> options = new List<QueryOption>
            {
                new QueryOption("$top", "20"),
            };

            return (await graphServiceClient.Users.Request(options).GetAsync().ConfigureAwait(false)).ToList();
        }
    }
}