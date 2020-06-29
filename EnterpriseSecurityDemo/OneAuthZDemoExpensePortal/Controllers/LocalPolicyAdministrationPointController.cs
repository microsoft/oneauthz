namespace ITAuthorizeDemoExpensePortal.Controllers
{
    using System;
    using System.Threading.Tasks;
    using ExpenseDemo.Models;
    using ExpenseDemo.Services.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Authorization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [Authorize(AuthenticationSchemes = "AdminUI")]
    [Produces("application/json")]
    [Route("api/LocalPolicyAdministrationPoint")]
    [EnableCors("LocalPAPCors")]
    public class LocalPolicyAdministrationPointController : Controller
    {
        private readonly IPolicyAdministrationPoint policyAdministrationPoint;

        public LocalPolicyAdministrationPointController(IPolicyAdministrationPoint policyAdministrationPoint)
        {
            this.policyAdministrationPoint = policyAdministrationPoint;
        }

        [HttpGet("UserApplications")]
        public Task<JObject> GetUserApplications()
        {
            return this.policyAdministrationPoint.GetUserApplications();
        }

        [HttpGet]
        [Route("RoleDefinitions/{appId}")]
        public async Task<IActionResult> GetRoleDefinitions(Guid appId)
        {
            return this.Ok(await this.policyAdministrationPoint.GetRoleDefinitions());
        }

        [HttpPut]
        [Route("RoleDefinition/{appId}")]
        public async Task<IActionResult> PutRoleDefinition(Guid appId, [FromBody] RoleDefinition roleDefinition)
        {
            try
            {
                return this.Ok(await this.policyAdministrationPoint.PutRoleDefinition(roleDefinition));
            }
            catch(ArgumentException ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("RoleDefinition/{appId}/{id}")]
        public async Task<IActionResult> DeleteRoleDefinition(Guid appId, Guid id)
        {
            try
            {
                var result = await this.policyAdministrationPoint.DeleteRoleDefinition(id);
                return result == null ? this.NoContent() as IActionResult : this.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        [HttpPost("SearchRoleAssignments")]
        public async Task<IActionResult> SearchRoleAssignments(Guid appId, string skipToken, [FromBody] JObject roleAssignmentSearchParameters)
        {
            return this.Ok(await this.policyAdministrationPoint.SearchRoleAssignments(JsonConvert.DeserializeObject<LooseRoleAssignmentSearchParameters>(roleAssignmentSearchParameters["searchParams"].ToString())));
        }

        [HttpPut]
        [Route("RoleAssignment/{appId}")]
        public async Task<IActionResult> PutRoleAssignment(Guid appId, [FromBody] RoleAssignment roleAssignment)
        {
            try
            {
                return this.Ok(await this.policyAdministrationPoint.PutRoleAssignment(roleAssignment));
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("RoleAssignment/{appId}/{id}")]
        public async Task<IActionResult> DeleteRoleAssignment(Guid appId, Guid id)
        {
            try
            {
                var result = await this.policyAdministrationPoint.DeleteRoleAssignment(id);
                return result == null ? this.NoContent() as IActionResult : this.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest(ex.Message);
            }
        }
    }
}