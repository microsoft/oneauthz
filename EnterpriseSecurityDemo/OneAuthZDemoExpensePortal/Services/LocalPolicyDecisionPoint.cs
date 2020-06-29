namespace ExpenseDemo.Services
{
    using System.Threading.Tasks;
    using ExpenseDemo.Services.Interfaces;
    using Microsoft.Enterprise.Authorization.Client.Experimental;
    using Microsoft.IdentityModel.Authorization.Internal.Pdp;

    public class LocalPolicyDecisionPoint : IPolicyDecisionPoint
    {
        private readonly LocalAuthorizationClient authorizationClient;

        public LocalPolicyDecisionPoint(LocalAuthorizationClient authorizationClient)
        {
            this.authorizationClient = authorizationClient;
        }
        public Task<bool> CheckAccess(CheckAccessRequest request)
        {
            return Task.FromResult(this.authorizationClient.CheckAccess(request).IsAccessGranted);
        }
    }
}
