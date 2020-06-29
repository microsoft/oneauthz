namespace ExpenseDemo.Models
{
    public class ExpenseDemoOptions
    {
        public Mode Mode { get; set; }

        public AttributeStoreOptions AttributeStoreOptions { get; set; } = new AttributeStoreOptions();
        public ServiceOptions GraphAPIOptions { get; set; } = new ServiceOptions();
        public ServiceOptions KustoQueryOptions { get; set; } = new ServiceOptions();

        public ClientOptions ClientOptions { get; set; } = new ClientOptions();

        public DocumentDBOptions DocumentDBOptions { get; set; } = new DocumentDBOptions();
    }

    public class DocumentDBOptions
    {
        public string Endpoint { get; set; }
        public string AuthKey { get; set; }
        public string DatabaseId { get; set; }
        public string CollectionId { get; set; }
        public string UseDocumentDB { get; set; }
    }

    public class ServiceOptions
    {
        public string BaseUrl { get; set; }
        public string ResourceUrl { get; set; }
    }

    public class AttributeStoreOptions : ServiceOptions
    {
        public string Namespace { get; set; }
    }

    public enum Mode
    {
        Service,
        Local
    }
}
