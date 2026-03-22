using Microsoft.EntityFrameworkCore.Storage;

namespace CommunityBankTellerAPI.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        ICustomerRepository Customers { get; }
        IAccountRepository Accounts { get; }
        ITransactionRepository Transactions { get; }
        ILedgerRepository LedgerEntries { get; }
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
