namespace ExpenseDemo.Services
{
    using ExpenseDemo.Models;
    using ExpenseDemo.Services.Interfaces;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class StaticKustoService : IAzureKustoService
    {
        public Task<KustoResult> GetKustoAuditLog(KustoQueryBody kustoQueryBody)
        {
            return Task.FromResult(new KustoResult
            {
                Columns = new List<KustoResultColumn>
                {
                    new KustoResultColumn
                    {
                        ColumnName = "Message"
                    }
                },
                Rows = new List<List<string>>
                {
                    new List<string>
                    {
                        "Audit Logs are not available when running in Local mode"
                    }
                }
            });
        }
    }
}
