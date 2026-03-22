using CommunityBankTellerAPI.Data;
using CommunityBankTellerAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace CommunityBankTellerAPI.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        // private readonly ApplicationDbContext _context; and other repos
        private readonly AppDbContext _context;
        private readonly ICustomerRepository _customerRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILedgerRepository _ledgerRepository;

        // constructor
        public UnitOfWork(AppDbContext context, ICustomerRepository customerRepository, IAccountRepository accountRepository, ITransactionRepository transactionRepository, ILedgerRepository ledgerRepository)
        {
            _context = context;
            _customerRepository = customerRepository;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _ledgerRepository = ledgerRepository;
        }

        public ICustomerRepository Customers => _customerRepository;

        public IAccountRepository Accounts => _accountRepository;

        public ITransactionRepository Transactions => _transactionRepository;

        public ILedgerRepository LedgerEntries => _ledgerRepository;

        public Task<IDbContextTransaction> BeginTransactionAsync() => _context.Database.BeginTransactionAsync();

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}