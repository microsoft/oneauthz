namespace ExpenseDemo.Models
{
    using System;
    using Microsoft.Enterprise.Authorization.Client.DataModels;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class LooseRoleAssignmentSearchParameters
    {
        [JsonProperty("direction")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScopeSearchDirection Direction { get; set; }
        [JsonProperty("scope")]
        public string Scope { get; set; }
        [JsonProperty("principalId")]
        public Guid? PrincipalId { get; set; }
        [JsonProperty("expandPrincipalGroups")]
        public bool ExpandPrincipalGroups { get; set; }
        [JsonProperty("roleDefinitionId")]
        public Guid? RoleDefinitionId { get; set; }
    }
}
