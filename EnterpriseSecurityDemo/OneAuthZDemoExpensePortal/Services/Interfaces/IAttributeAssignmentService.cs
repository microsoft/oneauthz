namespace ExpenseDemo.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ExpenseDemo.Models;

    public interface IAttributeAssignmentService
    {
        Task<IEnumerable<AttributeEntityAssignment>> GetAttributeAssignments(UserMetadata userMetadata);
    }
}
