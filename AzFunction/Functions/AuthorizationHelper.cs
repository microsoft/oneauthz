// -------------------------------------------------------------------------------
// <copyright file="AuthorizationHelper.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>


namespace Functions
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityModel.Client;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Enterprise.Authorization.Client;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using Microsoft.IdentityModel.Authorization;
    using Microsoft.IdentityModel.Authorization.Internal.Pdp;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// Wrapper support for invoking OneAuthZ CheckAccessAsync()
    /// </summary>
    public class AuthorizationHelper : IAuthorizationHelper
    {
        /// <summary>
        /// Holds app provided authorization client
        /// </summary>
        private readonly IAuthorizationClient client;

        /// <summary>
        /// Settings info populated into typed data model class.
        /// </summary>
        private readonly AuthorizationClientOptions options;

        /// <summary>
        /// Id provider settings populated into a typed data model class.
        /// </summary>
        private readonly IdentityProviderOptions idProviderOptions;

        /// <summary>
        /// Output logging endpoint (not the SDK logger)
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="optionsAccessor"></param>
        /// <param name="idProviderAccessor"></param>
        /// <param name="logger"></param>
        public AuthorizationHelper(
            IAuthorizationClient client,
            IOptions<AuthorizationClientOptions> optionsAccessor,
            IOptions<IdentityProviderOptions> idProviderAccessor,
            ILogger logger)
        {
            this.client = client;
            this.options = optionsAccessor.Value;
            this.idProviderOptions = idProviderAccessor.Value;
            this.logger = logger;
        }

        /// <summary>
        /// Utilizes OneAuthZ authorization SDK to validate user access.
        /// Direct inspection of the JWT token is required since Azure function host
        /// does not supply the subject (ObjectId) of the caller.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="resource"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task<bool> CheckAccessAysnc(
            HttpRequest req,
            string resource,
            string action)
        {
            StringValues headerValue;
            req.Headers.TryGetValue(Constants.authorizationHeader, out headerValue);

            var principle = await ValidateToken(headerValue.FirstOrDefault()).ConfigureAwait(false);
            var objectId = GetObjectId(principle);

            var subjectInfo = new SubjectInfo();
            subjectInfo.AddAttribute(Constants.objectIdKey, objectId);
            var resourceInfo = new ResourceInfo(resource);
            var actionInfo = new ActionInfo(action);

            var request = new CheckAccessRequest(
                subjectInfo,
                resourceInfo,
                actionInfo,
                null,
                options.MiddlewareOptions.GetMemberGroups);

            return (await client.CheckAccessAsync(request).ConfigureAwait(false)).IsAccessGranted;
        }

        /// <summary>
        /// Extracts the subject (principal) from the token if token is valid.
        /// </summary>
        /// <param name="authorizationHeaderValue"></param>
        /// <returns></returns>
        private async Task<ClaimsPrincipal> ValidateToken(string authorizationHeaderValue)
        {
            ClaimsPrincipal principal = null;

            if (String.IsNullOrEmpty(authorizationHeaderValue))
            {
                return principal;
            }

            // Strip off "Bearer" from Authorization header value
            var token = authorizationHeaderValue.Split(' ')[1];

            // Enable richer logging in development environment
            if (String.Compare(Environment.GetEnvironmentVariable(
                Constants.runtimeEnvironmentVariable),
                Constants.runtimeEnvironmentValue,
                StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                IdentityModelEventSource.ShowPII = true;
            }

            // Issuer keys must be supplied for validation
            IEnumerable<SecurityKey> keys = await GetIssuerKeys().ConfigureAwait(false);

            SecurityToken validatedToken;
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidAudience = idProviderOptions.Audience,
                ValidIssuer = idProviderOptions.Issuer,
                IssuerSigningKeys = keys
            };

            // Retuens a ClaimsPrincipal object
            return new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out validatedToken);
        }

        /// <summary>
        /// Uses well known ADD endpoints for obtaining AAD issuer keys
        /// </summary>
        /// <returns></returns>
        private async Task<List<SecurityKey>> GetIssuerKeys()
        {
            var keys = new List<SecurityKey>();

            // Read webkeys from discovery key endpoint
            var client = new HttpClient();
            var response = await client.GetJsonWebKeySetAsync(idProviderOptions.DiscoveryEndpoint).ConfigureAwait(false);

            if (!response.IsError)
            {
                foreach (var webKey in response.KeySet.Keys)
                {
                    var e = Base64Url.Decode(webKey.E);
                    var n = Base64Url.Decode(webKey.N);

                    var key = new RsaSecurityKey(new RSAParameters { Exponent = e, Modulus = n })
                    {
                        KeyId = webKey.Kid
                    };

                    keys.Add(key);
                }
            }

            return keys;
        }

        /// <summary>
        /// Retrieves the objectId of the principle from the list of claims
        /// </summary>
        /// <param name="principle"></param>
        /// <returns></returns>
        private string GetObjectId(ClaimsPrincipal principle)
        {
            if (principle.Identity == null ||
               !principle.Identity.IsAuthenticated ||
               !principle.HasClaim(c => c.Type == Constants.objectIdClaimKey))
            {
                throw new InvalidOperationException("Unable to retrieve object id from the claimsPrincipal.");
            }
            return principle.FindFirst(c => c.Type == Constants.objectIdClaimKey).Value;
        }
    }
}
