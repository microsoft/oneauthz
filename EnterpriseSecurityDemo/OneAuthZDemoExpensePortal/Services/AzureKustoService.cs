namespace ExpenseDemo.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using ExpenseDemo.Models;
    using ExpenseDemo.Services.Interfaces;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class AzureKustoService : IAzureKustoService
    {
        private readonly ExpenseDemoOptions options;

        public AzureKustoService(IOptions<ExpenseDemoOptions> optionsAccessor)
        {
            this.options = optionsAccessor.Value;
        }

        public async Task<KustoResult> GetKustoAuditLog(KustoQueryBody kustoQueryBody)
        {
            var token = await this.options.ClientOptions.GetAppTokenAsync(this.options.KustoQueryOptions.ResourceUrl);
            var resultString = await KustoHttpRequest("Kusto", new Uri(this.options.KustoQueryOptions.BaseUrl), token, kustoQueryBody);

            if (string.IsNullOrEmpty(resultString))
            {
                return null;
            }

            var result = JArray.Parse(resultString);
            var primaryResult = result.FirstOrDefault(
                table => table["TableName"] != null && table["TableName"].ToString() == "PrimaryResult");
            var kustoResult = primaryResult != null
                ? new KustoResult
                {
                    Columns = primaryResult["Columns"].ToObject<IEnumerable<KustoResultColumn>>(),
                    Rows = primaryResult["Rows"].ToObject<IEnumerable<IEnumerable<string>>>()
                }
                : null;
            return kustoResult;
        }

        private static async Task<string> KustoHttpRequest(string serverName, Uri uri, string token, KustoQueryBody body)
        {
            HttpResponseMessage result;
            string resultString;

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            result = await httpClient.PostAsync(uri, new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));
            resultString = await result.Content.ReadAsStringAsync();
            result.EnsureSuccessStatusCode();

            return resultString;
        }
    }
}
