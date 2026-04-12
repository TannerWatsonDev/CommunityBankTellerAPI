using CommunityBankTellerAPI.DTOs;

namespace CommunityBankTellerAPI.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionResponse> DepositAsync(DepositRequest request);
        Task<TransactionResponse> WithdrawAsync(WithdrawRequest request);
        Task<TransactionResponse> TransferAsync(TransferRequest request);
        Task<IEnumerable<TransactionResponse>> GetTransactionsByAccountIdAsync(int accountId);
    }
}