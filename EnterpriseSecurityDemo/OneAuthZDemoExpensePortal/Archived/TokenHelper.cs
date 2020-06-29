//namespace ExpenseDemo.Helpers
//{
//    using System;
//    using System.Configuration;
//    using System.Threading.Tasks;
//    using Microsoft.IdentityModel.Clients.ActiveDirectory;
//    using ITAuthorizeDemoExpensePortal.Helpers;

//    internal static class TokenHelper
//    {
//        private static readonly string Tenant = ConfigHelper.Configuration["DemoPet/Tenant"];
//        private static readonly string AppClientId = ConfigHelper.Configuration["DemoPet/ClientId"];
//        private static readonly string AppClientSecret = ConfigHelper.Configuration["DemoPet/ClientSecret"];

//        private static readonly string ITAAudience = ConfigHelper.Configuration["ITAuthorize/Audience"];

//        private static readonly string GraphAccessClientId = ConfigHelper.Configuration["GraphAccess/ClientId"];
//        private static readonly string GraphAccessClientSecret = ConfigHelper.Configuration["GraphAccess/ClientSecret"];

//        public static async Task<string> GetUserAuthenticationTokenAsync(string authenticationToken)
//        {
//            var authority = string.Format("https://login.microsoftonline.com/{0}", Tenant);
//            var authContext = new AuthenticationContext(authority);

//            var userAssertion = new UserAssertion(authenticationToken);
//            try
//            {
//                var credential = new ClientCredential(AppClientId, AppClientSecret);
//                var result = await authContext.AcquireTokenAsync("https://graph.windows.net/", credential, userAssertion);
//                return result.AccessToken;
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }

//        public static async Task<string> GetAuthenticationTokenAsync()
//        {
//            var authority = string.Format("https://login.microsoftonline.com/{0}", Tenant);
//            var authContext = new AuthenticationContext(authority);

//            var credential = new ClientCredential(AppClientId, AppClientSecret);
//            var result = await authContext.AcquireTokenAsync(ITAAudience, credential);
//            return result.AccessToken;
//        }

//        public static async Task<string> GetGraphAccessAuthenticationTokenAsync()
//        {
//            var authority = string.Format("https://login.microsoftonline.com/{0}", Tenant);
//            var authContext = new AuthenticationContext(authority);

//            var credential = new ClientCredential(GraphAccessClientId, GraphAccessClientSecret);
//            var result = await authContext.AcquireTokenAsync("https://graph.windows.net/", credential);
//            return result.AccessToken;
//        }
//    }
//}