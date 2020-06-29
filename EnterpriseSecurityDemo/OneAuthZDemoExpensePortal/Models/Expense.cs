namespace ExpenseDemo.Models
{
    using System;
    using Newtonsoft.Json;

    public class Expense
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "org")]
        public string Org { get; set; }

        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "owner")]
        public string Owner { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public int Amount { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "approver")]
        public string Approver { get; set; }

        [JsonProperty(PropertyName = "stateTimeStamp")]
        public DateTime StateTimeStamp { get; set; }

        [JsonProperty(PropertyName = "canApprove")]
        public bool CanApprove { get; set; }

        [JsonProperty(PropertyName = "canReject")]
        public bool CanReject { get; set; }
    }
}