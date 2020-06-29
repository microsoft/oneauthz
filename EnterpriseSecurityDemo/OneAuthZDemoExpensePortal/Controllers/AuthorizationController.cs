namespace ExpenseDemo.Controllers
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;
    using Microsoft.Extensions.Options;
    using ExpenseDemo.Authorization;
    using Microsoft.IdentityModel.Authorization;

    [Authorize]
    public class AuthorizationController : Controller
    {
        private readonly ExpenseDemoOptions options;
        private readonly AuthorizationService authorizationService;

        public AuthorizationController(IOptions<ExpenseDemoOptions> optionsAccessor, AuthorizationService authorizationService)
        {
            this.options = optionsAccessor.Value;
            this.authorizationService = authorizationService;
        }

        [HttpGet]
        [Route("api/Authorization/GetPermissions")]
        public async Task<Permissions> GetPermissions()
        {
            var submit = this.authorizationService.CheckAccess(new ResourceInfo("/ExpensePortal"), new ActionInfo("/Expense/Submit"), false, false);
            var audit = this.authorizationService.CheckAccess(new ResourceInfo("/ExpensePortal"), new ActionInfo("/Expense/Audit"), false, false);
            var resetAll = this.authorizationService.CheckAccess(new ResourceInfo("/ExpensePortal"), new ActionInfo("/Expense/ResetAll"), false, false);
            var DemoAs = this.authorizationService.CheckAccess(new ResourceInfo("/ExpensePortal"), new ActionInfo("/Portal/DemoAs"), false, false);
            return new Permissions
            {
                Submit = await submit,
                Audit = await audit,
                ResetAll = await resetAll,
                DemoAs = await DemoAs
            };
        }

        [HttpGet]
        [Route("api/Authorization/GetUserThumbnail")]
        public async Task<object> GetUserThumbnail()
        {
            var client = new HttpClient();
            var authorizationStr = this.Request.Headers["Authorization"].ToString();
            if(string.IsNullOrEmpty(this.options.ClientOptions.ClientSecret))
            {
                return null;
                
            }
            var token = await this.options.ClientOptions.GetUserTokenAsync(this.options.GraphAPIOptions.ResourceUrl, authorizationStr.Substring(authorizationStr.LastIndexOf(' ')).Trim());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var claimsIdentity = this.Request.HttpContext.User as ClaimsPrincipal;
            var upn = claimsIdentity.Identity.Name; 
            var portraitGraphUrl = string.Format("https://graph.windows.net/microsoft.com/users/{0}/thumbnailPhoto?api-version=1.6", upn);
            var response = await client.GetAsync(portraitGraphUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsByteArrayAsync();
            return content;
        }
    }
}