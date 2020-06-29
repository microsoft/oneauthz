

namespace ExpenseDemo.Services.Interfaces
{
    using ExpenseDemo.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IExpenseRepository
    {
        IEnumerable<Expense> GetCompleted();
        IEnumerable<Expense> GetPending();
        Task<Expense> Get(Guid id);
        Task DeleteAll();
        Task InitializeExpenses();
        Task AddOrUpdate(Expense expense);

    }
}
