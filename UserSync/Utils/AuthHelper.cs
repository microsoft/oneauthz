namespace UserSync.Utils
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.Enterprise.Authorization.Client;
    using Microsoft.IdentityModel.Authorization;
    using Microsoft.IdentityModel.Authorization.Internal.Pdp;

    public class AuthHelper
    {
        private static AuthorizationClientOptions AuthorizationClientOptions;
        internal const string ObjectIdentityType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        /// <summary>
        /// Validate access using OneAuthZ
        /// </summary>
        /// <param name="claimsIdentity">Identity</param>
        /// <returns></returns>
        internal static async Task<RemoteAuthorizationDecision> ValidateAccess(string resourceInfo, string actionInfo, ClaimsIdentity claimsIdentity)
        {
            using (AuthorizationClient authorizationClient = AuthHelper.GetAuthorizationClient())
            {
                // Request Parameters
                var oSubjectInfo = new SubjectInfo();

                // Add App/User Principal oid [Subject of an Access Check Determination]
                oSubjectInfo.AddAttribute("ObjectId", new Guid(claimsIdentity.Claims.First(x => x.Type == AuthHelper.ObjectIdentityType).Value));

                // Root resources
                var oResourceInfo = new ResourceInfo(resourceInfo);

                // NOTE: We are using Action as a reference key for a Role (RBAC tagging)
                var oActionInfo = new ActionInfo(actionInfo);

                // Request Object
                CheckAccessRequest oReq = new CheckAccessRequest(oSubjectInfo, oResourceInfo, oActionInfo, null, false, false);

                var authResponse = await authorizationClient.CheckAccessAsync(oReq).ConfigureAwait(false);

                return authResponse;
            }
        }

        internal static void InitializeAuthorizationClientOptions(AuthorizationClientOptions authorizationClientOptions)
        {
            AuthorizationClientOptions = authorizationClientOptions;
        }

        internal static AuthorizationClient GetAuthorizationClient()
        {
            return new AuthorizationClient(AuthorizationClientOptions);
        }
    }
}