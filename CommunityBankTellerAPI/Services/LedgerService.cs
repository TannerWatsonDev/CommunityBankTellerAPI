using CommunityBankTellerAPI.DTOs;
using CommunityBankTellerAPI.Repositories.Interfaces;
using CommunityBankTellerAPI.Services.Interfaces;

namespace CommunityBankTellerAPI.Services
{
    public class LedgerService : ILedgerService
    {
        // private variables for the services and context
        private readonly IUnitOfWork _unitOfWork;
        // constructor to inject the services and context
        public LedgerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<LedgerEntryResponse>> GetLedgerByAccountIdAsync(int accountId)
        {
            // verify accounts exist
            var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new KeyNotFoundException("Account not found");
            }

            // pull Ledger Entries
            var ledgers = await _unitOfWork.LedgerEntries.GetByAccountIdAsync(accountId);

            // return ledger response
            return ledgers.Select(l => new LedgerEntryResponse
            {
                Id = l.Id,
                AccountId = l.AccountId,
                TransactionId = l.TransactionId,
                BalanceBefore = l.BalanceBefore,
                BalanceAfter = l.BalanceAfter,
                Note = l.Note,
                CreatedAt = l.CreatedAt,
            });
        }
    }
}
