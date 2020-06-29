namespace ExpenseDemo.Services
{
    using System.Threading.Tasks;
    using ExpenseDemo.Services.Interfaces;
    using Newtonsoft.Json.Linq;

    public class StaticAttributeMetadataService : IAttributeMetadataService
    {
        public Task<JObject> GetAttributeCatalog()
        {
            return Task.FromResult(JObject.FromObject(new
            {
                Categories = new[]
                    {
                        "Food",
                        "Travel",
                        "Supply",
                        "Morale"
                    },
                Org = new[]
                    {
                        "CSEO",
                        "CSEO/CPE",
                        "CSEO/CPE/BPSC",
                        "CSEO/CPE/BAS",
                    }
            }));
        }
    }
}
