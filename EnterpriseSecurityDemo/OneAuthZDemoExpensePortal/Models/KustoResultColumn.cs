namespace ExpenseDemo.Models
{
    using Newtonsoft.Json;

    public class KustoResultColumn
    {
        [JsonProperty(PropertyName = "columnName")]
        public string ColumnName { get; set; }

        [JsonProperty(PropertyName = "columnType")]
        public string ColumnType { get; set; }
    }
}