namespace ExpenseDemo.Models
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    public class ClientOptions
    {
        public string Tenant { get; set; }
        public string TenantName { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public string Authority => $"https://sts.windows.net/{this.Tenant}";

        public string AdminUIClientId { get; set; }

        public async Task<string> GetAppTokenAsync(string resourceUrl)
        {
            var authContext = new AuthenticationContext($"https://login.microsoftonline.com/{this.Tenant}");
            var credential = new ClientCredential(this.ClientId, this.ClientSecret);
            var result = await authContext.AcquireTokenAsync(resourceUrl, credential);
            return result.AccessToken;
        }

        public async Task<string> GetUserTokenAsync(string resourceUrl, string userToken)
        {
            var authContext = new AuthenticationContext($"https://login.microsoftonline.com/{this.Tenant}");

            var userAssertion = new UserAssertion(userToken);
            try
            {
                var credential = new ClientCredential(this.ClientId, this.ClientSecret);
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
