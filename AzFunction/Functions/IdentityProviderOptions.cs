// <copyright file="IdentityProviderOptions.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------


namespace Functions
{
    public class IdentityProviderOptions
    {
        /// <summary>
        /// API resource
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Identity provider endpoint
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Endpoint for obtaining issuer public keys
        /// </summary>
        public string DiscoveryEndpoint { get; set; }
    }
}
