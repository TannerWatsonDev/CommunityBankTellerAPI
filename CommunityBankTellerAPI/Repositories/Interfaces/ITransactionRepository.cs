using CommunityBankTellerAPI.Models;

namespace CommunityBankTellerAPI.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(int id);
        Task<Transaction> AddAsync(Transaction transaction);
        Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId);
    }
}
