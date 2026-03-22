using CommunityBankTellerAPI.Data;
using CommunityBankTellerAPI.DTOs;
using CommunityBankTellerAPI.Models;
using CommunityBankTellerAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommunityBankTellerAPI.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _context;
        public TransactionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TransactionResponse> DepositAsync(DepositRequest request)
        {
            throw new NotImplementedException();

        }

        public async Task<IEnumerable<TransactionResponse>> GetTransactionsByAccountIdAsync(int accountId)
        {
            throw new NotImplementedException();
        }

        public async Task<TransactionResponse> TransferAsync(TransferRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<TransactionResponse> WithdrawAsync(WithdrawRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
