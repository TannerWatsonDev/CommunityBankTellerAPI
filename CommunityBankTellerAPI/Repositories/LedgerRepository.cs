using CommunityBankTellerAPI.Data;
using CommunityBankTellerAPI.Models;
using CommunityBankTellerAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommunityBankTellerAPI.Repositories
{
    public class LedgerRepository : ILedgerRepository
    {
        private readonly AppDbContext _context;
        public LedgerRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(LedgerEntry entry)
        {
            _context.LedgerEntries.Add(entry);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<LedgerEntry>> GetByAccountIdAsync(int accountId)
        {
            // retrieve ledger entries for the specified account ID
            return await _context.LedgerEntries
                .Where(l => l.AccountId == accountId)
                .OrderBy(l => l.CreatedAt)
                .ToListAsync();
        }
    }
}
