namespace ExpenseDemo.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using ExpenseDemo.Services.Interfaces;
    using Microsoft.Enterprise.Authorization.Client.Middleware;
    using ExpenseDemo.Authorization;
    using Microsoft.IdentityModel.Authorization;
    using static ExpenseDemo.Models.ExpensePortalConstant;

    [Authorize]
    public class ExpenseController : Controller
    {
        private readonly IExpenseRepository expenseRepository;
        private readonly AuthorizationService authorizationService;

        public ExpenseController(IExpenseRepository expenseRepository, AuthorizationService authorizationService)
        {
            this.expenseRepository = expenseRepository;
            this.authorizationService = authorizationService;
        }

        [HttpGet]
        [Route("api/Expenses/GetPendingExpenses")]
        public async Task<IActionResult> GetPendingExpenses()
        {
            var allPendingCandidates = this.expenseRepository.GetPending();
            if ((allPendingCandidates == null) || (allPendingCandidates.Count() == 0))
            {
                return new OkResult();
            }

            var canReadCandidate = await this.GetFilteredExpenseListForUser(allPendingCandidates);
            return new OkObjectResult(canReadCandidate);
        }

        private async Task<List<Expense>> GetFilteredExpenseListForUser(IEnumerable<Expense> unfilteredList)
        {
            var canReadCandidate = await this.FilterByCanRead(unfilteredList);
            await this.CheckApproveReject(canReadCandidate.ToList());
            return canReadCandidate;
        }

        private async Task<List<Expense>> FilterByCanRead(IEnumerable<Expense> candidates)
        {
            var tasks = candidates.Select(async c => new { Item = c, Criteria = await this.authorizationService.CheckAccess(PopulateResourceInfo(c), new ActionInfo("/Expense/Read"), true, true)});
            var allTasks = await Task.WhenAll(tasks);
            var expenses = allTasks.Where(t => t.Criteria).Select(t => t.Item);
            return expenses.ToList();
        }

        private async Task CheckApproveReject(List<Expense> candidates)
        {
            var approveTasks = candidates.Select(async c => new { Item = c, Criteria = await this.authorizationService.CheckAccess(PopulateResourceInfo(c), new ActionInfo("/Expense/Approve"), true, true) });
            var rejectTasks = candidates.Select(async c => new { Item = c, Criteria = await this.authorizationService.CheckAccess(PopulateResourceInfo(c), new ActionInfo("/Expense/Reject"), true, true) });
            var allApproveTasks = await Task.WhenAll(approveTasks);
            candidates.ForEach(candidate => candidate.CanApprove = allApproveTasks.First(t => t.Item == candidate).Criteria);
            var allRejectTasks = await Task.WhenAll(rejectTasks);
            candidates.ForEach(candidate => candidate.CanReject = allRejectTasks.First(t => t.Item == candidate).Criteria);
        }

        [HttpGet]
        [Route("api/Expenses/GetCompletedExpenses")]
        public async Task<IActionResult> GetCompletedExpenses()
        {
            var allPendingCandidates = this.expenseRepository.GetCompleted();
            if ((allPendingCandidates == null) || (allPendingCandidates.Count() == 0))
            {
                return new OkResult();
            }

            var canReadCandidate = await this.FilterByCanRead(allPendingCandidates);
            return new OkObjectResult(canReadCandidate);
        }

        [HttpPut]
        [Route("api/Expenses")]
        [AadCheckAccessAuthorize("/ExpensePortal", "/Expense/Submit")]
        public async Task<IActionResult> Submit([FromBody] Expense expense)
        {
            expense.Id = Guid.NewGuid();
            await this.expenseRepository.AddOrUpdate(expense);

            var canReadCandidate = await this.FilterByCanRead(new[] { expense });
            await this.CheckApproveReject(canReadCandidate.ToList());
            return new OkObjectResult(canReadCandidate.FirstOrDefault());
        }

        [HttpPut]
        [Route("api/Expenses/{id}/Approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var item = await this.expenseRepository.Get(id);
            var checkAccess = await this.authorizationService.CheckAccess(PopulateResourceInfo(item), new ActionInfo("/Expense/Approve"), true, true);
            if(!checkAccess)
            {
                return new ForbidResult();
            }

            if (string.IsNullOrEmpty(item.Approver))
            {
                item.State = "Approved";
                item.Approver = this.authorizationService.CurrentUser.Alias;
                item.StateTimeStamp = DateTime.UtcNow;
                await this.expenseRepository.AddOrUpdate(item);
                return new OkObjectResult(item);
            }

            return new BadRequestObjectResult("The state of the expense has been changed. Please refresh to get the latest data.");
        }

        [HttpPut]
        [Route("api/Expenses/{id}/Reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var item = await this.expenseRepository.Get(id);
            var checkAccess = await this.authorizationService.CheckAccess(PopulateResourceInfo(item), new ActionInfo("/Expense/Reject"), true, true);
            if (!checkAccess)
            {
                return new ForbidResult();
            }
            if (string.IsNullOrEmpty(item.Approver))
            {
                item.State = "Rejected";
                item.Approver = this.authorizationService.CurrentUser.Alias;
                item.StateTimeStamp = DateTime.UtcNow;
                await this.expenseRepository.AddOrUpdate(item);
                return new OkObjectResult(item);
            }

            return new BadRequestObjectResult("The state of the expense has been changed. Please refresh to get the latest data." );
        }

        private static ResourceInfo PopulateResourceInfo(Expense item)
        {
            var resourceInfo = new ResourceInfo($"/ExpensePortal/Orgs/{item.Org}/Categories/{item.Category}");
            resourceInfo.AddAttribute(Resource.AttributeName.OwnerOrg, item.Org);
            resourceInfo.AddAttribute(Resource.AttributeName.OwnerAlias, item.Owner);
            resourceInfo.AddAttribute(Resource.AttributeName.Amount, item.Amount);
            resourceInfo.AddAttribute(Resource.AttributeName.Category, item.Category);
            return resourceInfo;
        }

        [HttpDelete]
        [Route("api/Expenses/Reset")]
        [AadCheckAccessAuthorize("/ExpensePortal", "/Expense/ResetAll")]
        public async Task Reset()
        {
            await this.expenseRepository.DeleteAll();
            await this.expenseRepository.InitializeExpenses();
        }

        //private static async Task<string> GetGraphAccessAuthenticationTokenAsync()
        //{
        //    var authority = string.Format("https://login.microsoftonline.com/{0}", Tenant);
        //    var authContext = new AuthenticationContext(authority);

        //    var credential = new ClientCredential(GraphAccessClientId, GraphAccessClientSecret);
        //    var result = await authContext.AcquireTokenAsync("https://graph.windows.net/", credential);
        //    return result.AccessToken;
        //}

        //private static async Task<string> GetAuthenticationTokenAsync()
        //{
        //    var authority = string.Format("https://login.microsoftonline.com/{0}", Tenant);
        //    var authContext = new AuthenticationContext(authority);

        //    var credential = new ClientCredential(AppClientId, AppClientSecret);
        //    var result = await authContext.AcquireTokenAsync(ITAAudience, credential);
        //    return result.AccessToken;
        //}


        //private AccessRequest CreateResourceAccessRequest(Expense candidate, string action)
        //{
        //    return new AccessRequest
        //    {
        //        ActionName = action,
        //        ResourceName = "Expense",
        //        Scope = $"/ExpensePortal/Org/{candidate.Org}/Categories/{candidate.Category}",
        //        RequestAttributes = new[]
        //        {
        //            new RequestAttribute
        //            {
        //                Name = ExpensePortalConstant.Resource.AttributeName.Amount,
        //                Type = "Resource",
        //                Value = candidate.Amount.ToString()
        //            },
        //            new RequestAttribute
        //            {
        //                Name = ExpensePortalConstant.Resource.AttributeName.OwnerAlias,
        //                Type = "Resource",
        //                Value = candidate.Owner
        //            },
        //            new RequestAttribute
        //            {
        //                Name = ExpensePortalConstant.Resource.AttributeName.OwnerOrg,
        //                Type = "Resource",
        //                Value = candidate.Org
        //            },
        //            new RequestAttribute
        //            {
        //                Name = ExpensePortalConstant.Resource.AttributeName.Category,
        //                Type = "Resource",
        //                Value = candidate.Category
        //            }
        //        },
        //        RequestId = candidate.Id
        //    };
        //}

    }
}