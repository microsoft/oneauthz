namespace ExpenseDemo.Models
{ 
    using Newtonsoft.Json;

    public class KustoQueryBody
    {
        [JsonProperty(PropertyName = "db")]
        public string Db { get; set; }

        [JsonProperty(PropertyName = "csl")]
        public string Csl { get; set; }
    }
}