namespace ExpenseDemo.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents.Client;
    using Models;
    using Newtonsoft.Json;
    using ExpenseDemo.Services.Interfaces;
    using Microsoft.Extensions.Options;

    internal class ExpenseRepository : IExpenseRepository
    {
        private const string ExpenseInitializationData = "wwwroot/Storage/ExpenseInitializationData.json";
        private readonly Uri collectionLink;
        private readonly DocumentClient documentClient;
        private readonly ExpenseDemoOptions options;

        public ExpenseRepository(IOptions<ExpenseDemoOptions> optionsAccessor)
        {
            this.options = optionsAccessor.Value;
            this.documentClient = new DocumentClient(new Uri(this.options.DocumentDBOptions.Endpoint), this.options.DocumentDBOptions.AuthKey);
            this.collectionLink = UriFactory.CreateDocumentCollectionUri(this.options.DocumentDBOptions.DatabaseId, this.options.DocumentDBOptions.CollectionId);
        }

        public async Task AddOrUpdate(Expense expense)
        {
            await this.documentClient.UpsertDocumentAsync(this.collectionLink, expense);
        }

        public async Task InitializeExpenses()
        {
            var data = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), ExpenseInitializationData));
            var expenses = JsonConvert.DeserializeObject<List<Expense>>(data);
            expenses.ForEach(item => item.Id = Guid.NewGuid());
            var random = new Random();
            foreach (var item in expenses)
            {
                item.Amount = random.Next(1, 100);
                await this.documentClient.UpsertDocumentAsync(this.collectionLink, item);
            }
        }

        public async Task DeleteAll()
        {
            var collection = await this.documentClient.ReadDocumentCollectionAsync(this.collectionLink);
            var docs = this.documentClient.CreateDocumentQuery(collection.Resource.DocumentsLink);
            foreach (var doc in docs)
            {
                await this.documentClient.DeleteDocumentAsync(doc.SelfLink);
            }
        }

        public async Task<Expense> Get(Guid id)
        {
            return (dynamic)(await this.documentClient.ReadDocumentAsync(UriFactory.CreateDocumentUri(this.options.DocumentDBOptions.DatabaseId, this.options.DocumentDBOptions.CollectionId, id.ToString()))).Resource;
        }

        public IEnumerable<Expense> GetPending()
        {
            return this.documentClient.CreateDocumentQuery<Expense>(this.collectionLink)
                .Where(expense => (expense.State == null) || (expense.State == string.Empty))
                .AsEnumerable().ToList();
        }

        public IEnumerable<Expense> GetCompleted()
        {
            return this.documentClient.CreateDocumentQuery<Expense>(this.collectionLink)
                .Where(expense => (expense.State != null) && (expense.State != string.Empty))
                .AsEnumerable().ToList();
        }
    }
}