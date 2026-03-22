using CommunityBankTellerAPI.Models;

namespace CommunityBankTellerAPI.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByEmailAsync(string email);
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer> AddAsync(Customer customer);
        Task<Customer> UpdateAsync(Customer customer);
    }
}
