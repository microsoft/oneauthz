namespace ExpenseDemo.Models
{
    using System.Collections.Generic;
    using Microsoft.IdentityModel.Authorization;
    using Newtonsoft.Json;

    public class RoleAssignmentsByPages
    {
        [JsonProperty(PropertyName = "lstRoleAssignments")]
        public IEnumerable<RoleAssignment> RoleAssignments { get; set; }
        [JsonProperty(PropertyName = "skipToken")]
        public string SkipToken { get; set; }
    }
}
