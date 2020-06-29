namespace ExpenseDemo.Controllers
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using ExpenseDemo.Services.Interfaces;
    using Microsoft.Enterprise.Authorization.Client.Middleware;

    [Authorize]
    public class AttributeStoreController : Controller
    {
        private readonly IAttributeMetadataService attributeMetadataService;

        public AttributeStoreController(IAttributeMetadataService attributeMetadataService)
        {
            this.attributeMetadataService = attributeMetadataService;
        }

        [HttpGet]
        [Route("api/AttributeStore/GetDomainValues/")]
        [AadCheckAccessAuthorize("/ExpensePortal", "/Expense/Submit")]
        public Task<JObject> GetAttributeCatalog()
        {
            return this.attributeMetadataService.GetAttributeCatalog();

        }
    }
}