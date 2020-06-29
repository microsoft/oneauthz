// -------------------------------------------------------------------------------
// <copyright file="TokenGetter.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------

namespace Functions.Runner
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// Retrieves an id_token from AAD
    /// </summary>
    public class TokenGetter
    {
        /// <summary>
        /// Gets application token
        /// </summary>
        /// <param name="options">parameter must not be empty, provides AAD needed parameters</param>
        /// <param name="resourceUrl">parameter must not be empty, indicates API resource</param>
        /// <returns>token as string</returns>
        public static async Task<string> GetAppTokenAsync(AzureAdOptions options, string resourceUrl)
        {
            var idP = new Uri(new Uri($"https://{options.Authority}"), $"/{options.TenantId}");
            var authContext = new AuthenticationContext(idP.ToString());
            var credential = new ClientCredential(options.ClientId, options.ClientSecret);
            var result = await authContext.AcquireTokenAsync(resourceUrl, credential);
            return result.AccessToken;
        }

        /// <summary>
        /// Gets a user token based on previously issued token
        /// </summary>
        /// <param name="options">parameter must not be empty, provides AAD needed parameters</param>
        /// <param name="resourceUrl">parameter must not be empty, indicates API resource</param>
        /// <param name="userToken">parameter must not be empty, previously issued user token</param>
        /// <returns>token value as string</returns>
        public static async Task<string> GetUserTokenAsync(AzureAdOptions options, string resourceUrl, string userToken)
        {
            var idP = new Uri(new Uri($"https://{options.Authority}"), $"/{options.TenantId}");
            var authContext = new AuthenticationContext(idP.ToString());

            var userAssertion = new UserAssertion(userToken);
            try
            {
                var credential = new ClientCredential(options.ClientId, options.ClientSecret);
                var result = await authContext.AcquireTokenAsync(resourceUrl, credential, userAssertion);
                return result.AccessToken;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
