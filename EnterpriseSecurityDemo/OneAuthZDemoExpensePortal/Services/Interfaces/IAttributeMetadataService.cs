namespace ExpenseDemo.Services.Interfaces
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    public interface IAttributeMetadataService
    {
        Task<JObject> GetAttributeCatalog();
    }
}
