namespace ExpenseDemo.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ExpenseDemo.Models;
    using ExpenseDemo.Services.Interfaces;
    using Microsoft.Enterprise.Authorization.Client.Experimental;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Authorization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class LocalPolicyAdministrationPoint : IPolicyAdministrationPoint
    {
        private readonly LocalAuthorizationClientOptions localAuthorizationClientOptions;
        private readonly LocalPolicyStore localPolicyStore;

        public LocalPolicyAdministrationPoint(IOptions<LocalAuthorizationClientOptions> optionsAccessor)
        {
            this.localAuthorizationClientOptions = optionsAccessor.Value;
            var policyDataString = File.ReadAllText(this.localAuthorizationClientOptions.PolicyFilePath);
            this.localPolicyStore = JsonConvert.DeserializeObject<LocalPolicyStore>(policyDataString);
        }

        public Task<JObject> GetUserApplications()
        {
            var result = new
            {
                systemRoleDefs = new[] 
                {
                    new
                    {
                    }
                },
                applications = new[]
                {
                    new
                    {
                        tenant = Guid.Empty,
                        appId = Guid.Empty,
                        displayName = "Local Policy Store",
                        systemRoles = new string[] { "Policy Administrator" }
                    }
                }
            };
            return Task.FromResult(JObject.FromObject(result));
        }

        public Task<IEnumerable<RoleDefinition>> GetRoleDefinitions()
        {
            return Task.FromResult(this.localPolicyStore.RoleDefinitions as IEnumerable<RoleDefinition>);
        }

        public Task<RoleDefinition> PutRoleDefinition(RoleDefinition roleDefinition)
        {
            var possibleRoleDefinitionWithDuplicateName = this.localPolicyStore.RoleDefinitions.FirstOrDefault(rd => rd.Name == roleDefinition.Name);
            if (possibleRoleDefinitionWithDuplicateName != null && roleDefinition.Id != possibleRoleDefinitionWithDuplicateName.Id)
            {
                throw new ArgumentException("Duplicate Role Definition Name", nameof(roleDefinition));
            }

            lock (this.localPolicyStore)
            {
                var existingRoleDefinitionIndex = this.localPolicyStore.RoleDefinitions.FindIndex(rd => rd.Id == roleDefinition.Id.ToString());
                if (existingRoleDefinitionIndex != -1)
                {
                    this.localPolicyStore.RoleDefinitions[existingRoleDefinitionIndex] = roleDefinition;
                }
                else
                {
                    this.localPolicyStore.RoleDefinitions.Add(roleDefinition);
                }
                this.SavePolicyData();
            }
            return Task.FromResult(roleDefinition);
        }

        public Task<RoleDefinition> DeleteRoleDefinition(Guid id)
        {
            if(this.localPolicyStore.RoleAssignments.Any(ra => ra.RoleDefinitionId == id.ToString()))
            {
                throw new ArgumentException("A role definition can't be deleted until all assingments are deleted.", nameof(id));
            }

            lock (this.localPolicyStore)
            {
                var existingRoleDefinition = this.localPolicyStore.RoleDefinitions.Find(rd => rd.Id == id.ToString());
                if (existingRoleDefinition != null)
                {
                    this.localPolicyStore.RoleDefinitions.Remove(existingRoleDefinition);
                    this.SavePolicyData();
                    return Task.FromResult(existingRoleDefinition);
                }
                else
                {
                    return Task.FromResult<RoleDefinition>(null);
                }
            }     
        }

        public Task<RoleAssignmentsByPages> SearchRoleAssignments(LooseRoleAssignmentSearchParameters roleAssignmentSearchParameters)
        {
            var result = this.localPolicyStore.RoleAssignments.Where(ra => 
                (roleAssignmentSearchParameters.PrincipalId.HasValue && roleAssignmentSearchParameters.PrincipalId != Guid.Empty ? ra.PrincipalId == roleAssignmentSearchParameters.PrincipalId.ToString() : true) &&
                (roleAssignmentSearchParameters.RoleDefinitionId.HasValue && roleAssignmentSearchParameters.RoleDefinitionId != Guid.Empty ? ra.RoleDefinitionId == roleAssignmentSearchParameters.RoleDefinitionId.ToString() : true)
            );
            return Task.FromResult(new RoleAssignmentsByPages
            {
                RoleAssignments = result
            });
        }

        public Task<RoleAssignment> PutRoleAssignment(RoleAssignment roleAssignment)
        {
            var possibleDuplicateRoleAssignment = this.localPolicyStore.RoleAssignments.FirstOrDefault(ra => ra.RoleDefinitionId == roleAssignment.RoleDefinitionId && ra.Scope == roleAssignment.Scope && ra.PrincipalId == roleAssignment.PrincipalId);
            if (possibleDuplicateRoleAssignment != null && roleAssignment.Id != possibleDuplicateRoleAssignment.Id)
            {
                throw new ArgumentException("Can't insert a duplicate role assignment", nameof(roleAssignment));
            }


            lock (this.localPolicyStore)
            {
                roleAssignment.PrincipalType = "User";
                var existingRoleAssigmentIndex = this.localPolicyStore.RoleAssignments.FindIndex(ra => ra.Id == roleAssignment.Id);
                if (existingRoleAssigmentIndex != -1)
                {
                    this.localPolicyStore.RoleAssignments[existingRoleAssigmentIndex] = roleAssignment;
                }
                else
                {
                    this.localPolicyStore.RoleAssignments.Add(roleAssignment);
                }
                this.SavePolicyData();
            }
            return Task.FromResult(roleAssignment);
        }

        public Task<RoleAssignment> DeleteRoleAssignment(Guid id)
        {
            lock (this.localPolicyStore)
            {
                var existingRoleAssignment = this.localPolicyStore.RoleAssignments.Find(ra => ra.Id == id.ToString());
                if (existingRoleAssignment != null)
                {
                    this.localPolicyStore.RoleAssignments.Remove(existingRoleAssignment);
                    this.SavePolicyData();
                    return Task.FromResult(existingRoleAssignment);
                }
                else
                {
                    return Task.FromResult<RoleAssignment>(null);
                }
            }
        }

        private void SavePolicyData()
        {
            var policyDataString = JsonConvert.SerializeObject(this.localPolicyStore, Formatting.Indented);
            File.WriteAllText(this.localAuthorizationClientOptions.PolicyFilePath, policyDataString);
        }

    }
}
