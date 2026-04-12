using CommunityBankTellerAPI.Models;
using CommunityBankTellerAPI.DTOs;

namespace CommunityBankTellerAPI.Services.Interfaces
{
    public interface ICustomerService
    {
            Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);
            Task<CustomerResponse> GetCustomerByIdAsync(int id);
            Task<CustomerResponse> UpdateCustomerAsync(int id, UpdateCustomerRequest request);
    }
}
