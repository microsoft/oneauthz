namespace ExpenseDemo.Authorization
{
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Enterprise.Authorization.Client.Internal;
    using Microsoft.Enterprise.Authorization.Client.Middleware;
    using Microsoft.IdentityModel.Authorization;

    internal class ExpenseAuthorizationHandler : AuthorizationHandler<AadAuthorizationCheckAccessRequirement>
    {
        private readonly AuthorizationService authorizationService;
        private readonly ILogger logger;
        private const string ObjectIdKey = "ObjectId";

        public ExpenseAuthorizationHandler(AuthorizationService authorizationService, ILogger logger)
        {
            this.authorizationService = authorizationService;
            this.logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AadAuthorizationCheckAccessRequirement requirement)
        {
            var authorizationFilterContext = context.Resource as AuthorizationFilterContext;
            var controllerActionDescriptor = authorizationFilterContext?.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor == null)
            {
                this.logger?.Log(TraceLevel.Error, "AadAuthorizationHandler error: Unable to get MVC ControllerActionDescriptor");
                return;
            }

            var aadAuthorizationMappingAttribute = ((AadCheckAccessAuthorizeAttribute[])controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(AadCheckAccessAuthorizeAttribute), true)).FirstOrDefault();
            if (aadAuthorizationMappingAttribute == null)
            {
                this.logger?.Log(TraceLevel.Error, "AadAuthorizationHandler: Unable to retrieve AadCheckAccessAuthorizeAttribute on the calling method");
                return;
            }

            var resourceId = aadAuthorizationMappingAttribute.ResourceId;
            var actionId = aadAuthorizationMappingAttribute.ActionId;

            var response = await this.authorizationService.CheckAccess(new ResourceInfo(resourceId), new ActionInfo(actionId), false, false).ConfigureAwait(false);
            if (response)
            {
                context.Succeed(requirement);
            }
        }
    }
}
