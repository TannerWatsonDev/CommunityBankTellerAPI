using CommunityBankTellerAPI.Models;

namespace CommunityBankTellerAPI.Repositories.Interfaces
{
    public interface ILedgerRepository
    {
        Task AddAsync(LedgerEntry entry);

        Task<IEnumerable<LedgerEntry>> GetByAccountIdAsync(int accountId);
    }
}