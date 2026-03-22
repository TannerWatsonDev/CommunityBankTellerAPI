using CommunityBankTellerAPI.Models;

namespace CommunityBankTellerAPI.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(int id);
        Task<Account> AddAsync(Account account);
        Task<Account> UpdateAsync(Account account);
        Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(int customerId);
    }
}
