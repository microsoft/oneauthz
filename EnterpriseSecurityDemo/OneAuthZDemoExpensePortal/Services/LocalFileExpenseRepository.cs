namespace ExpenseDemo.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;
    using Newtonsoft.Json;
    using System.Collections.Concurrent;
    using System.Threading;
    using ExpenseDemo.Services.Interfaces;

    internal class LocalFileExpenseRepository : IExpenseRepository
    {
        private const string LocalExpenseFileName = "wwwroot/Storage/LocalExpenseStorage.json";
        private const string ExpenseInitializationData = "wwwroot/Storage/ExpenseInitializationData.json";
        private readonly ConcurrentDictionary<Guid, List<Expense>> concurrent;
        private readonly Semaphore semaphore;
        public LocalFileExpenseRepository()
        {
            this.concurrent = new ConcurrentDictionary<Guid, List<Expense>>();
            this.semaphore = new Semaphore(1, 1);
        }

        public async Task AddOrUpdate(Expense expense)
        {
            try
            {
                this.semaphore.WaitOne();
                List<Expense> upsertExpenseList = null;
                using (var sr = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), LocalExpenseFileName)))
                {
                    upsertExpenseList = JsonConvert.DeserializeObject<List<Expense>>(sr.ReadToEnd());
                }
                var upsertExpenseDict = upsertExpenseList == null ? new List<Expense>().ToDictionary(x => x.Id) : upsertExpenseList.ToDictionary(x => x.Id);
                upsertExpenseDict[expense.Id] = expense;
                await this.SaveExpense(upsertExpenseDict.Values);
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        public async Task InitializeExpenses()
        {
            this.semaphore.WaitOne();
            try
            {
                var data = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), ExpenseInitializationData));
                var expenses = JsonConvert.DeserializeObject<List<Expense>>(data);
                var random = new Random();
                expenses.ForEach(item => {
                    item.Id = Guid.NewGuid();
                    item.Amount = random.Next(1, 100);

                });
                await this.SaveExpense(expenses);
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        public async Task DeleteAll()
        {
            try
            {
                this.semaphore.WaitOne();
                await this.SaveExpense(new List<Expense>());
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        public Task<Expense> Get(Guid id)
        {
            try
            {
                this.semaphore.WaitOne();
                using (var sr = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), LocalExpenseFileName)))
                {
                    var existingExpenseList = JsonConvert.DeserializeObject<List<Expense>>(sr.ReadToEnd());
                    return Task.FromResult(existingExpenseList.FirstOrDefault(x => x.Id == id));
                }
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        public IEnumerable<Expense> GetPending()
        {
            try
            {
                this.semaphore.WaitOne();
                using (var sr = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), LocalExpenseFileName)))
                {
                    var existingExpenseList = JsonConvert.DeserializeObject<List<Expense>>(sr.ReadToEnd());
                    if (existingExpenseList == null)
                    {
                        return new List<Expense>();
                    }
                    return existingExpenseList.Where(x => string.IsNullOrEmpty(x.State));
                }
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        public IEnumerable<Expense> GetCompleted()
        {
            try
            {
                this.semaphore.WaitOne();
                using (var sr = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), LocalExpenseFileName)))
                {
                    var existingExpenseList = JsonConvert.DeserializeObject<List<Expense>>(sr.ReadToEnd());
                    if (existingExpenseList == null)
                    {
                        return new List<Expense>();
                    }
                    return existingExpenseList.Where(x => !string.IsNullOrEmpty(x.State));
                }
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        private async Task SaveExpense(IEnumerable<Expense> expense)
        {
            var expenses = JsonConvert.SerializeObject(expense, Formatting.Indented);
            using (var sw = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), LocalExpenseFileName)))
            {
                await sw.WriteAsync(expenses);
            }
        }
    }
}