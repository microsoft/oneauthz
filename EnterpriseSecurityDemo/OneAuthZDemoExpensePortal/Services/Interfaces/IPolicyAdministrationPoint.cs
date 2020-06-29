namespace ExpenseDemo.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ExpenseDemo.Models;
    using Microsoft.IdentityModel.Authorization;
    using Newtonsoft.Json.Linq;

    public interface IPolicyAdministrationPoint
    {
        Task<JObject> GetUserApplications();
        Task<IEnumerable<RoleDefinition>> GetRoleDefinitions();
        Task<RoleDefinition> PutRoleDefinition(RoleDefinition roleDefinition);
        Task<RoleDefinition> DeleteRoleDefinition(Guid id);
        Task<RoleAssignmentsByPages> SearchRoleAssignments(LooseRoleAssignmentSearchParameters roleAssignmentSearchParameters);
        Task<RoleAssignment> PutRoleAssignment(RoleAssignment roleAssignment);
        Task<RoleAssignment> DeleteRoleAssignment(Guid id);
    }
}