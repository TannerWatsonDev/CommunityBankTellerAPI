using CommunityBankTellerAPI.DTOs;

namespace CommunityBankTellerAPI.Services.Interfaces
{
    public interface ILedgerService
    {
        Task<IEnumerable<LedgerEntryResponse>> GetLedgerByAccountIdAsync(int accountId);
    }
}
