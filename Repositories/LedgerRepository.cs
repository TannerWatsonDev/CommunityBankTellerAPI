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
    }
}
