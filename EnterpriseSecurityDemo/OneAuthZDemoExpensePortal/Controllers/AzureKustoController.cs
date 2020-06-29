namespace ExpenseDemo.Controllers
{
    using System.Threading.Tasks;
    using ExpenseDemo.Models;
    using ExpenseDemo.Services.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Enterprise.Authorization.Client.Middleware;

    [Authorize]
    public class AzureKustoController : Controller
    {
        private readonly IAzureKustoService azureKustoService;

        public AzureKustoController(IAzureKustoService azureKustoService)         
        {
            this.azureKustoService = azureKustoService;
        }

        [HttpPost]
        [Route("api/AzureKusto/KustoQuery/")]
        [AadCheckAccessAuthorize("/ExpensePortal", "/Expense/Audit")]
        public Task<KustoResult> KustoQuery([FromBody]KustoQueryBody kustoQueryBody)
        {
            return this.azureKustoService.GetKustoAuditLog(kustoQueryBody);
        }
    }
}