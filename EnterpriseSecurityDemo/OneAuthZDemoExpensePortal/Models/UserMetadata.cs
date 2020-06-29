namespace ExpenseDemo.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public class UserMetadata
    {
        [JsonProperty(PropertyName = "objectId")]
        public string ObjectId { get; set; }

        [JsonProperty(PropertyName = "givenName")]
        public string GivenName { get; set; }

        [JsonProperty(PropertyName = "surname")]
        public string Surname { get; set; }

        [JsonProperty(PropertyName = "userPrincipalName")]
        public string UserPrincipalName { get; set; }

        [JsonProperty(PropertyName = "extension_18e31482d3fb4a8ea958aa96b662f508_BuildingName")]
        public string BuildingName { get; set; }

        [JsonProperty(PropertyName = "extension_18e31482d3fb4a8ea958aa96b662f508_ReportsToEmailName")]
        public string ReportsToEmailName { get; set; }

        [JsonProperty(PropertyName = "extension_18e31482d3fb4a8ea958aa96b662f508_ReportsToPersonnelNbr")]
        public string ReportsToPersonnelNbr { get; set; }

        [JsonProperty(PropertyName = "extension_18e31482d3fb4a8ea958aa96b662f508_CostCenterCode")]
        public string CostCenterCode { get; set; }

        [JsonProperty(PropertyName = "extension_18e31482d3fb4a8ea958aa96b662f508_ProfitCenterCode")]
        public string ProfitCenterCode { get; set; }

        [JsonProperty(PropertyName = "mailNickname")]
        public string Alias { get; set; }

        // The logic behind these three properties are for demo purposes only and not intended for production. For production usage, customers are advised to leverage extensionattribute2 available from MS Graph.
        public string IsMSFTE => (this.UserPrincipalName.EndsWith(MicrosoftSuffix) && !this.UserPrincipalName.Contains("-")).ToString();
        public string IsMSNonFTE => (this.UserPrincipalName.EndsWith(MicrosoftSuffix) && this.UserPrincipalName.Contains("-") && !this.UserPrincipalName.Contains("t-")).ToString();
        public string IsMSIntern => (this.UserPrincipalName.EndsWith(MicrosoftSuffix) && this.UserPrincipalName.Contains("t-")).ToString();

        private const string MicrosoftSuffix = "@microsoft.com";

        [JsonProperty(PropertyName = "extension_18e31482d3fb4a8ea958aa96b662f508_ApprovedSAFELimitAmt")]
        public string GraphSafeLimit { get; set; }

        //public int SafeLimit { get; set; }

        //public string Org { get; set; }

        //public List<string> Categories { get; set; }

        public UserMetadata()
        {

        }

        public UserMetadata(IEnumerable<AttributeEntityAssignment> attributeEntityAssignment)
        {
            var properties = this.GetType().GetProperties();

            // Exclucde certain properties
            foreach (var pi in properties.Where(p => p.GetCustomAttributes(typeof(JsonPropertyAttribute), false).Any()))
            {
                var jsonPropertyName = (pi.GetCustomAttributes(typeof(JsonPropertyAttribute), false).First() as JsonPropertyAttribute).PropertyName;
                var value = attributeEntityAssignment.FirstOrDefault(a => string.Equals(jsonPropertyName, a.Name, StringComparison.OrdinalIgnoreCase))?.Values.FirstOrDefault();
                pi.SetValue(this, value);
            }
        }

        public static List<string> GetJsonAttributeNames()
        {
            var properties = typeof(UserMetadata).GetProperties();
            return properties.Where(p => p.GetCustomAttributes(typeof(JsonPropertyAttribute), false).Any()).Select(p => (p.GetCustomAttributes(typeof(JsonPropertyAttribute), false).First() as JsonPropertyAttribute).PropertyName).ToList();
        }
    }
}