namespace ExpenseDemo.Models
{
    using System.Collections.Generic;
    using Microsoft.IdentityModel.Authorization;

    public class LocalPolicyStore
    {
        public List<RoleDefinition> RoleDefinitions { get; set; }
        public List<RoleAssignment> RoleAssignments { get; set; }
    }
}
