// -------------------------------------------------------------------------------
// <copyright file="TokenGetter.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------

namespace UserSyncCore
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// Helper class to retrieve a token for the given resource.
    /// </summary>
    internal class TokenGetter
    {
        /// <summary>
        /// URN identifier for the type of request from issuer
        /// </summary>
        private const string OauthRequestType = "urn:ietf:params:oauth:grant-type:jwt-bearer";

        /// <param name="idToken">
        /// The hostname
        /// </param>
        private readonly string idToken;

        /// <param name="authorityHostName">
        /// The hostname
        /// </param>
        private readonly string authorityHostName;

        /// <param name="clientId">
        /// The client id
        /// </param>
        private readonly string clientId;

        /// <param name="clientSecret">
        /// The certificate name
        /// </param>
        private readonly string clientSecret;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenGetter"/> class.
        /// </summary>
        /// <param name="idToken">
        /// idToken provided in the HttpContext of the incoming HttpRequest
        /// </param>
        /// <param name="authorityHostName">
        /// The hostname
        /// </param>
        /// <param name="clientId">
        /// The client id
        /// </param>
        /// <param name="clientSecret">
        /// Shared secret for validation of connection with AAD
        /// </param>
        internal TokenGetter(
            string idToken,
            string authorityHostName,
            string clientId,
            string clientSecret)
        {
            this.idToken = idToken;
            this.authorityHostName = authorityHostName;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        /// <summary>
        /// Gets the access token of a service/application
        /// (i.e. this test application ) to a given resource inside the tenant.
        /// </summary>
        /// <param name="resource">
        /// The resource id
        /// </param>
        /// <param name="claimsPrincipal">
        /// The claimsPrincipal for whom a token is being requested
        /// </param>
        /// <returns>
        /// A Task<string> object that when completed, has the access token
        /// </returns>
        internal async Task<string> AcquireTokenOnBehalfOfAsync(string resource, ClaimsPrincipal claimsPrincipal)
        {
            var clientCred = new ClientCredential(this.clientId, this.clientSecret);

            var userName = claimsPrincipal.FindFirst(ClaimTypes.Upn) != null
                ? claimsPrincipal.FindFirst(ClaimTypes.Upn).Value
                : claimsPrincipal.FindFirst(ClaimTypes.Email).Value;

            var userAssertion = new UserAssertion(
                this.idToken,
                OauthRequestType,
                userName);

            var context = new AuthenticationContext(this.authorityHostName);

            var result = await context.AcquireTokenAsync(
                resource,
                clientCred,
                userAssertion).ConfigureAwait(false);

            return result?.AccessToken;
        }
    }
}
