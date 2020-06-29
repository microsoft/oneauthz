namespace ExpenseDemo.Services
{
    using System.Threading.Tasks;
    using ExpenseDemo.Services.Interfaces;
    using Microsoft.Enterprise.Authorization.Client;
    using Microsoft.IdentityModel.Authorization.Internal.Pdp;

    public class RemotePolicyDecisionPoint : IPolicyDecisionPoint
    {
        private readonly IAuthorizationClient authorizationClient;

        public RemotePolicyDecisionPoint(IAuthorizationClient authorizationClient)
        {
            this.authorizationClient = authorizationClient;
        }
        public async Task<bool> CheckAccess(CheckAccessRequest request)
        {
            return (await this.authorizationClient.CheckAccessAsync(request).ConfigureAwait(false)).IsAccessGranted;
        }
    }
}
