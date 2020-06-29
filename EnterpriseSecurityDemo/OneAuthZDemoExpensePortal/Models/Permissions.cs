using Newtonsoft.Json;

namespace ExpenseDemo.Models
{
    public class Permissions
    {
        [JsonProperty("submit")]
        public bool Submit { get; set; }
        [JsonProperty("audit")]
        public bool Audit { get; set; }
        [JsonProperty("resetAll")]
        public bool ResetAll { get; set; }
        [JsonProperty("demoAs")]
        public bool DemoAs { get; set; }

    }
}
