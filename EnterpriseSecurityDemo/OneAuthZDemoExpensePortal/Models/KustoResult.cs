namespace ExpenseDemo.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class KustoResult
    {
        [JsonProperty(PropertyName = "columns")]
        public IEnumerable<KustoResultColumn> Columns { get; set; }

        [JsonProperty(PropertyName = "rows")]
        public IEnumerable<IEnumerable<string>> Rows { get; set; }
    }
}