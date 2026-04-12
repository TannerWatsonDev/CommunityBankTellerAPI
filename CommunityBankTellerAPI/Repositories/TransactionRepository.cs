using CommunityBankTellerAPI.Data;
using CommunityBankTellerAPI.Models;
using CommunityBankTellerAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommunityBankTellerAPI.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        // create a private field for the database context
        private readonly AppDbContext _context;

        // constructor to inject the database context
        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction> AddAsync(Transaction transaction)
        {
            // add the transaction to the database context
            _context.Transactions.Add(transaction);
            // save the changes to the database
            await _context.SaveChangesAsync();
            // return the added transaction
            return transaction;
        }

        public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId)
        {
            // retrieve transactions for the specified account ID, ordered by creation date descending
            return await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {
            // retrieve a transaction by its ID
            return await _context.Transactions.FindAsync(id);
        }
    }
}