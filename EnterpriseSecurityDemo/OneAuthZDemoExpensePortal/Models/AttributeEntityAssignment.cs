namespace ExpenseDemo.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class AttributeEntityAssignment
    {
        [JsonProperty(PropertyName = "assignmentNamespace")]
        public string AssignmentNamespace { get; set; }

        [JsonProperty(PropertyName = "assignmentBusinessDomain")]
        public string AssignmentBusinessDomain { get; set; }

        [JsonProperty(PropertyName = "entity")]
        public string Entity { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "values")]
        public List<string> Values { get; set; }
    }
}