using Microsoft.EntityFrameworkCore;
using CommunityBankTellerAPI.Models;

namespace CommunityBankTellerAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<LedgerEntry> LedgerEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(a => a.Balance).HasPrecision(18, 2);
            });

            modelBuilder.Entity<LedgerEntry>(entity =>
            {
                entity.Property(l => l.BalanceBefore).HasPrecision(18, 2);
                entity.Property(l => l.BalanceAfter).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(t => t.Amount).HasPrecision(18, 2);

                entity.HasOne(t => t.Account)
                    .WithMany(a => a.Transactions)
                    .HasForeignKey(t => t.AccountId);

                entity.HasOne(t => t.RelatedAccount)
                    .WithMany()
                    .HasForeignKey(t => t.RelatedAccountId);
            });
        }
    }
}
