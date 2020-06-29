// -------------------------------------------------------------------------------
// <copyright file="AzureAdOptions.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------

namespace Functions.Runner
{
    /// <summary>
    /// Settings information for AAD
    /// </summary>
    public class AzureAdOptions
    {
        /// <summary>
        /// Gets or sets ClientId property
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets ClientSecret property
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets audience property - Production is: login.microsoftonline.com
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets Client app domain
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets TenantId property
        /// </summary>
        public string TenantId { get; set; }
    }
}
