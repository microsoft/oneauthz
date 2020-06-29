namespace ExpenseDemo.Services
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ExpenseDemo.Models;
    using ExpenseDemo.Services.Interfaces;
    using Newtonsoft.Json;

    public class LocalAttributeStore : IAttributeAssignmentService
    {
        private const string LocalAttributeStoreFile = "wwwroot/Storage/LocalAttributeStore.json";

        public LocalAttributeStore()
        {

        }
        public Task<IEnumerable<AttributeEntityAssignment>> GetAttributeAssignments(UserMetadata userMetadata)
        {
            var assignments = this.LoadAttributeAssignments();
            if (!assignments.ContainsKey(userMetadata.UserPrincipalName))
            {
                return Task.FromResult(null as IEnumerable<AttributeEntityAssignment>);
            }
            var result = assignments[userMetadata.UserPrincipalName].Select(kvp => new AttributeEntityAssignment
            {
                Name = kvp.Key,
                Values = new List<string> { kvp.Value }
            }).ToList();
            return Task.FromResult(result as IEnumerable<AttributeEntityAssignment>);
        }

        private Dictionary<string, Dictionary<string,string>> LoadAttributeAssignments()
        {
            var content = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), LocalAttributeStoreFile));
            return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(content);
        }
    }
}
