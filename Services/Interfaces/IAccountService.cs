using CommunityBankTellerAPI.DTOs;

namespace CommunityBankTellerAPI.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request);

        Task<AccountResponse> GetAccountByIdAsync(int accountId);

        Task<AccountResponse> CloseAccountByIdAsync(int accountId);
        Task<IEnumerable<AccountResponse>> GetAccountsByCustomerIdAsync(int customerId);
    }
}
