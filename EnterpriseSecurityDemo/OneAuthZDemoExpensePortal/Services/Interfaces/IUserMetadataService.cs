namespace ExpenseDemo.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ExpenseDemo.Models;

    public interface IUserMetadataService
    {
        Task<UserMetadata> GetUserGraph(string userIdentifier);

        Task<List<string>> GetUserReportingChain(string userPrincipalName);
    }
}
