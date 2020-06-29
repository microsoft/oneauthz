namespace ExpenseDemo.Authorization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using ExpenseDemo.Models;
    using ExpenseDemo.Services.Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Enterprise.Authorization.Client;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Authorization;
    using Microsoft.IdentityModel.Authorization.Internal.Pdp;

    public class AuthorizationService
    {
        private readonly HttpContext httpContext;
        private readonly IPolicyDecisionPoint policyDecisionPoint;
        private readonly IUserMetadataService userMetadataService;
        private readonly IAttributeAssignmentService attributeAssignmentService;
        private readonly IMemoryCache memoryCache;
        private readonly AuthorizationClientOptions authorizationClientOptions;
        private readonly ExpenseDemoOptions expenseDemoOptions;

        public UserMetadata CurrentUser => this.currentUser.Value;
        private readonly Lazy<UserMetadata> currentUser;

        private IEnumerable<AttributeEntityAssignment> CurrentUserAttributeAssignments => this.currentUserAttributeAssignments.Value;
        private readonly Lazy<IEnumerable<AttributeEntityAssignment>> currentUserAttributeAssignments;

        private bool ImpersonationCheckAccessResult => this.impersonationCheckAccessResult.Value;
        private readonly Lazy<bool> impersonationCheckAccessResult;

        private const string ObjectIdClaimKey = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        private const string ObjectIdKey = "ObjectId";
        private const string UserPrincipalNameKey = "UserPrincipalName";
        private const string ImpersonationHeader = "ImpersonationUser";
        private const string ImpersonationResourceId = "/ExpensePortal";
        private const string ImpersonationActionId = "/Portal/DemoAs";

        public AuthorizationService(IOptions<AuthorizationClientOptions> authorizationClientOptionsAccessor, IOptions<ExpenseDemoOptions> expenseDemoOptionsAccessor, IHttpContextAccessor httpContextAccessor, IPolicyDecisionPoint policyDecisionPoint, IUserMetadataService userMetadataService, IAttributeAssignmentService attributeAssignmentService, IMemoryCache memoryCache)
        {
            this.authorizationClientOptions = authorizationClientOptionsAccessor.Value;
            this.expenseDemoOptions = expenseDemoOptionsAccessor.Value;
            this.httpContext = httpContextAccessor.HttpContext;
            this.policyDecisionPoint = policyDecisionPoint;
            this.userMetadataService = userMetadataService;
            this.attributeAssignmentService = attributeAssignmentService;
            this.memoryCache = memoryCache;
            this.currentUser = new Lazy<UserMetadata>(() => this.PopulateCurrentUser().Result);
            this.currentUserAttributeAssignments = new Lazy<IEnumerable<AttributeEntityAssignment>>(() => this.PopulateCurrentUserAttributeAssignments().Result);
            this.impersonationCheckAccessResult = new Lazy<bool>(() => this.ImpersonationCheckAccess().Result);

        }

        private async Task<UserMetadata> PopulateCurrentUser()
        {
            var objectId = GetObjectId(this.httpContext.User);
            var impersonationUser = GetImpersonationUser(this.httpContext);

            if (!string.IsNullOrEmpty(impersonationUser))
            {
                return await this.userMetadataService.GetUserGraph($"{impersonationUser}@{this.expenseDemoOptions.ClientOptions.TenantName}").ConfigureAwait(false);
            }
            else
            {
                return await this.userMetadataService.GetUserGraph(objectId).ConfigureAwait(false);
            }
        }

        private async Task<IEnumerable<AttributeEntityAssignment>> PopulateCurrentUserAttributeAssignments()
        {
            return await this.attributeAssignmentService.GetAttributeAssignments(this.CurrentUser);
        }

        private async Task<bool> ImpersonationCheckAccess()
        {
            if (this.expenseDemoOptions.Mode == Mode.Local)
            {
                return true;
            }

            var objectId = GetObjectId(this.httpContext.User);
            var impersonationSubjectInfo = new SubjectInfo();
            impersonationSubjectInfo.AddAttribute(ObjectIdKey, objectId);
            impersonationSubjectInfo.AddAttribute(UserPrincipalNameKey, this.httpContext.User.Identity.Name);

            var impersonationResourceInfo = new ResourceInfo(ImpersonationResourceId);
            var impersonationActionInfo = new ActionInfo(ImpersonationActionId);
            var impersonationResultRequest = new CheckAccessRequest(impersonationSubjectInfo, impersonationResourceInfo, impersonationActionInfo, null, this.authorizationClientOptions.MiddlewareOptions.GetMemberGroups);

            var impersonationResult = await this.policyDecisionPoint.CheckAccess(impersonationResultRequest).ConfigureAwait(false);

            return impersonationResult;
        }


        public async Task<bool> CheckAccess(ResourceInfo resourceInfo, ActionInfo actionInfo, bool pullAttributeAssignments, bool pullReportingChain)
        {
            // Impersonation Access Check
            var impersonationUser = GetImpersonationUser(this.httpContext);
            if (!string.IsNullOrEmpty(impersonationUser))
            {
                // If attempting to impersonate but has no access to do so
                if (!this.ImpersonationCheckAccessResult)
                {
                    return false;
                }
                // If attempting to impersonate and has access and the action being checked is impersonation
                else if (actionInfo.Id == ImpersonationActionId)
                {
                    return true;
                }
            }

            var subjectInfo = new SubjectInfo();
            this.AddUserMetadataToSubjectInfo(subjectInfo, this.CurrentUser);

            if (pullAttributeAssignments)
            {
                if(this.CurrentUserAttributeAssignments != null)
                {
                    foreach (var attributeAssignment in this.CurrentUserAttributeAssignments)
                    {
                        // Special cases
                        if (attributeAssignment.Name == ExpensePortalConstant.Subject.AttributeName.SafeLimit)
                        {
                            // Add as integer
                            var safeLimit = int.Parse(attributeAssignment.Values.First());
                            subjectInfo.AddAttribute(attributeAssignment.Name, safeLimit);
                        }
                        else if (attributeAssignment.Name == ExpensePortalConstant.Subject.AttributeName.Org)
                        {
                            // Append * to the end for wild card comparison
                            subjectInfo.AddAttribute(attributeAssignment.Name, $"{attributeAssignment.Values.First()}*");
                        }
                        else
                        {
                            attributeAssignment.Values.Where(v => !string.IsNullOrEmpty(v)).ToList().ForEach(v => subjectInfo.AddAttribute(attributeAssignment.Name, v));
                        }
                    }
                }              
            }

            if (pullReportingChain)
            {
                List<string> managers; 
                lock (this.memoryCache)
                {
                    managers = this.memoryCache.Get(this.CurrentUser.UserPrincipalName) as List<string>;
                    if (managers == null)
                    {
                        // Note: Use .Result for now as await can't be used inside a lock
                        managers = this.userMetadataService.GetUserReportingChain(this.CurrentUser.UserPrincipalName).Result;
                        this.memoryCache.Set(this.CurrentUser.UserPrincipalName, managers);
                    }
                }

                foreach (var manager in managers)
                {
                    subjectInfo.AddAttribute(ExpensePortalConstant.Subject.AttributeName.ReportingChain, manager);
                }

                var owner = resourceInfo.GetAttribute(ExpensePortalConstant.Resource.AttributeName.OwnerAlias).First().ToString();
                if (string.IsNullOrEmpty(owner))
                {
                    throw new ArgumentException("Resource.OwnerAlias is required when pulling reporting chain.");
                }

                List<string> ownerManagers;
                lock (this.memoryCache)
                {
                    ownerManagers = this.memoryCache.Get($"{owner}@{this.expenseDemoOptions.ClientOptions.TenantName}") as List<string>;
                    if (ownerManagers == null)
                    {
                        // Note: Use .Result for now as await can't be used inside a lock
                        ownerManagers = this.userMetadataService.GetUserReportingChain($"{owner}@{this.expenseDemoOptions.ClientOptions.TenantName}").Result;
                        this.memoryCache.Set($"{owner}@{this.expenseDemoOptions.ClientOptions.TenantName}", ownerManagers);
                    }
                }

                foreach (var manager in ownerManagers)
                {
                    resourceInfo.AddAttribute(ExpensePortalConstant.Resource.AttributeName.OwnerReportingChain, manager);
                };
            }

            return (await this.policyDecisionPoint.CheckAccess(new CheckAccessRequest(subjectInfo, resourceInfo, actionInfo, null, this.authorizationClientOptions.MiddlewareOptions.GetMemberGroups)).ConfigureAwait(false));
        }

        private static string GetObjectId(ClaimsPrincipal user)
        {
            if (user.Identity == null || !user.Identity.IsAuthenticated || !user.HasClaim(c => c.Type == ObjectIdClaimKey))
            {
                throw new InvalidOperationException("Unable to retrieve object id from the claimsPrincipal");
            }
            return user.FindFirst(c => c.Type == ObjectIdClaimKey).Value;
        }

        private static string GetImpersonationUser(HttpContext context)
        {
            var header = context.Request.Headers[ImpersonationHeader].FirstOrDefault();
            if (string.IsNullOrEmpty(header))
            {
                return null;
            }
            return header;
        }

        private void AddUserMetadataToSubjectInfo(SubjectInfo subject, UserMetadata userMetadata)
        {
            // Add all properties from Graph Object Info
            var properties = userMetadata.GetType().GetProperties();

            // Exclucde certain properties
            foreach (var pi in properties)
            {
                var value = (string)pi.GetValue(userMetadata);
                if (!string.IsNullOrEmpty(value))
                {
                    subject.AddAttribute(pi.Name, value);
                }
            }
        }
    }
}
