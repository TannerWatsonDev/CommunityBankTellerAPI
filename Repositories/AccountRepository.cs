using CommunityBankTellerAPI.Data;
using CommunityBankTellerAPI.Models;
using CommunityBankTellerAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommunityBankTellerAPI.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        // create a private field for the database context
        private readonly AppDbContext _context;

        // constructor to inject the database context
        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Account> AddAsync(Account account)
        {
            // add the account to the database context
            _context.Accounts.Add(account);
            // save the changes to the database
            await _context.SaveChangesAsync();
            // return the added account
            return account;
        }

        public async Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(int customerId)
        {
            // retrieve accounts for the specified customer ID
            return await _context.Accounts
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            // retrieve an account by its ID
            return await _context.Accounts.FindAsync(id);
        }

        public async Task<Account> UpdateAsync(Account account)
        {
            // update the account in the database context
            _context.Accounts.Update(account);
            // save the changes to the database
            await _context.SaveChangesAsync();
            // return the updated account
            return account;
        }
    }
}
