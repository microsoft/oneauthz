// -------------------------------------------------------------------------------
// <copyright file="IAuthorizationHelper.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>


namespace Functions
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Public interface for AuthorizationHelper
    /// </summary>
    public interface IAuthorizationHelper
    {
        Task<bool> CheckAccessAysnc(HttpRequest req, string resource, string action);
    }
}
