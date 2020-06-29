
namespace ExpenseDemo.Services.Interfaces
{
    using ExpenseDemo.Models;
    using System.Threading.Tasks;

    public interface IAzureKustoService
    {
        Task<KustoResult> GetKustoAuditLog(KustoQueryBody kustoQueryBody);
    }
}