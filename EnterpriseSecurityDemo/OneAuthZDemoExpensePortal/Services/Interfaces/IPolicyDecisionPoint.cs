namespace ExpenseDemo.Services.Interfaces
{
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Authorization.Internal.Pdp;

    public interface IPolicyDecisionPoint
    {
        Task<bool> CheckAccess(CheckAccessRequest request);
    }
}
