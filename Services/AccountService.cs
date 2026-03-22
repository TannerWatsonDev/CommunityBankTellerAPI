using CommunityBankTellerAPI.DTOs;
using CommunityBankTellerAPI.Models;
using CommunityBankTellerAPI.Repositories.Interfaces;
using CommunityBankTellerAPI.Services.Interfaces;

namespace CommunityBankTellerAPI.Services
{
    public class AccountService : IAccountService
    {
        // private variable for database context through DAL
        private readonly IAccountRepository _accountRepository;
        private readonly ICustomerRepository _customerRepository;

        // constructor to inject the database context
        public AccountService(IAccountRepository accountRepository, ICustomerRepository customerRepository)
        {
            _accountRepository = accountRepository;
            _customerRepository = customerRepository;
        }


        /// <summary>
        /// Asynchronously creates a new account for the specified customer.
        /// </summary>
        /// <remarks>The new account is created with an initial balance of 0 and an active status. The
        /// account number is generated after the account is saved to ensure uniqueness.</remarks>
        /// <param name="request">The details of the account to create, including the customer ID and account type. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an AccountResponse with the
        /// details of the newly created account.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the customer specified by request.CustomerId does not exist.</exception>
        public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request)
        {
            // validate customer exists
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {request.CustomerId} not found.");
            }

            // create new account
            var account = new Account
            {
                // account number generated after saving to database to ensure uniqueness
                AccountNumber = string.Empty,
                Type = request.Type,
                Status = AccountStatus.Active,
                Balance = 0m,
                CreatedAt = DateTime.UtcNow,
                CustomerId = request.CustomerId
            };

            // add and save to database
            await _accountRepository.AddAsync(account);


            // generate unique account number (e.g. "ACC" + zero-padded account ID)
            account.AccountNumber = $"ACC{account.Id:D8}";
            // update account with generated account number
            await _accountRepository.UpdateAsync(account);

            // map to response DTO
            return new AccountResponse
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                Type = account.Type,
                Status = account.Status,
                Balance = account.Balance,
                CreatedAt = account.CreatedAt,
                ClosedAt = account.ClosedAt,
                CustomerId = account.CustomerId
            };
        }


        /// <summary>
        /// Asynchronously retrieves account details for the specified account identifier.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AccountResponse"/>
        /// with the account details if found.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no account with the specified <paramref name="accountId"/> exists.</exception>
        public async Task<AccountResponse> GetAccountByIdAsync(int accountId)
        {
            // validate account exists
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new KeyNotFoundException($"Account with ID {accountId} not found.");
            }

            // map to response DTO
            return new AccountResponse
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                Type = account.Type,
                Status = account.Status,
                Balance = account.Balance,
                CreatedAt = account.CreatedAt,
                ClosedAt = account.ClosedAt,
                CustomerId = account.CustomerId
            };
        }


        /// <summary>
        /// Closes the account with the specified identifier and returns the updated account information.
        /// </summary>
        /// <remarks>The account's status is set to Closed and the closure timestamp is recorded. This
        /// operation is irreversible; once closed, the account cannot be reopened.</remarks>
        /// <param name="accountId">The unique identifier of the account to close. Must refer to an existing, open account.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an AccountResponse object with
        /// the updated account details after closure.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if an account with the specified accountId does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the account with the specified accountId is already closed.</exception>
        public async Task<AccountResponse> CloseAccountByIdAsync(int accountId)
        {
            // validate account exists and is open
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new KeyNotFoundException($"Account with ID {accountId} not found.");
            }
            if (account.Status == AccountStatus.Closed)
            {
                throw new InvalidOperationException($"Account with ID {accountId} is already closed.");
            }

            // update account status and closed date
            account.Status = AccountStatus.Closed;
            account.ClosedAt = DateTime.UtcNow;

            await _accountRepository.UpdateAsync(account);

            // map to response DTO
            return new AccountResponse
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                Type = account.Type,
                Status = account.Status,
                Balance = account.Balance,
                CreatedAt = account.CreatedAt,
                ClosedAt = account.ClosedAt,
                CustomerId = account.CustomerId
            };
        }


        /// <summary>
        /// Asynchronously retrieves all accounts associated with the specified customer identifier.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer whose accounts are to be retrieved. Must correspond to an existing
        /// customer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of account
        /// responses for the specified customer. The collection is empty if the customer has no accounts.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no customer with the specified identifier exists.</exception>
        public async Task<IEnumerable<AccountResponse>> GetAccountsByCustomerIdAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
            }

            var accounts = await _accountRepository.GetAccountsByCustomerIdAsync(customerId);

            return accounts.Select(a => new AccountResponse
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                Type = a.Type,
                Status = a.Status,
                Balance = a.Balance,
                CreatedAt = a.CreatedAt,
                ClosedAt = a.ClosedAt,
                CustomerId = a.CustomerId
            });
        }
    }
}
