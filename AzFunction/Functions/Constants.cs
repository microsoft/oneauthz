// -------------------------------------------------------------------------------
// <copyright file="Constants.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Functions
{
    /// <summary>
    /// Static strings
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Used for development (local testing) purposes
        /// </summary>
        public const string runtimeEnvironmentVariable = "ASPNETCORE_ENVIRONMENT";

        /// <summary>
        /// Used for development (local testing) purposes
        /// </summary>
        public const string runtimeEnvironmentValue = "Development";

        /// <summary>
        /// Used for development (local testing) purposes
        /// </summary>
        public const string globalMutexName = "Global\\Functions.Runner";

        /// <summary>
        /// Settings file
        /// </summary>
        public const string appSettingsFile = "local.settings.json";

        /// <summary>
        /// Required settings for section
        /// </summary>
        public const string azureAdSettingsSection = "AzureAd";

        /// <summary>
        /// Required settings for section
        /// </summary>
        public const string authClientSettingsSection = "AuthorizationClient";

        /// <summary>
        /// Required settings for section
        /// </summary>
        public const string identityProvider = "IdentityProvider";

        /// <summary>
        /// Header key for obtaining JWT token
        /// </summary>
        public const string authorizationHeader = "Authorization";

        /// <summary>
        /// Claims key specifying the objectId of the subject
        /// </summary>
        public const string objectIdClaimKey = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        /// <summary>
        /// Expected parameter key for specifying the objectId for CheckAccess calls the PDP
        /// </summary>
        public const string objectIdKey = "ObjectId";

    }
}
