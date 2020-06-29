// -------------------------------------------------------------------------------
// <copyright file="Constants.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Functions
{
    /// <summary>
    /// Static strings
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Expected query string parameter name
        /// </summary>
        public const string QueryStringParameter = "name";

        /// <summary>
        /// Sample value for query string parameter 'name'
        /// </summary>
        public const string QueryStringValue = "Bill";

        /// <summary>
        /// Prefix to authorization header
        /// </summary>
        public const string AuthenticationHeader = "Bearer";

        /// <summary>
        /// API resource
        /// </summary>
        public const string FunctionsResource = "api://5f6a73f2-37ba-4259-9de2-d6bc3df36768";

        /// <summary>
        /// Interprocess mutex name
        /// </summary>
        public const string GlobalMutexName = "Global\\Functions.Runner";

        /// <summary>
        /// Settings file
        /// </summary>
        public const string AppSettingsFile = "appsettings.json";

        /// <summary>
        /// Settings file section
        /// </summary>
        public const string AzureAdSettingsSection = "AzureAd";

        /// <summary>
        /// Functions HTTPTrigger endpoint
        /// </summary>
        public const string FunctionsEndpoint = "FunctionsEndPoint";

        /// <summary>
        /// Another HTTPTrigger endpoint
        /// </summary>
        public const string Functions2EndPoint = "Functions2EndPoint";
    }
}
