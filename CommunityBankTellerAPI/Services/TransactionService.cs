using CommunityBankTellerAPI.DTOs;
using CommunityBankTellerAPI.Exceptions;
using CommunityBankTellerAPI.Models;
using CommunityBankTellerAPI.Repositories.Interfaces;
using CommunityBankTellerAPI.Services.Interfaces;
using System.Security.Principal;

namespace CommunityBankTellerAPI.Services
{
    public class TransactionService : ITransactionService
    {
        // private variables for the services and context
        private readonly IUnitOfWork _unitOfWork;
        // constructor to inject the services and context
        public TransactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Asynchronously deposits the specified amount into an account and records the transaction.
        /// </summary>
        /// <remarks>The deposit operation is performed within a database transaction to ensure atomicity.
        /// A ledger entry is created for each deposit. The account balance is updated only after the transaction is
        /// successfully committed.</remarks>
        /// <param name="request">The deposit request containing the account identifier, amount to deposit, and an optional description. The
        /// account must exist and be active. The amount must be a positive value.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResponse with
        /// details of the completed deposit transaction.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the account specified in the request does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the account specified in the request is not active.</exception>
        public async Task<TransactionResponse> DepositAsync(DepositRequest request)
        {
            // Get account by id, verify it exists
            var account = await _unitOfWork.Accounts.GetByIdAsync(request.AccountId);
            if (account == null)
            {
                throw new KeyNotFoundException("Account not found");
            }
            // check if account is active
            if (account.Status != AccountStatus.Active)
            {
                throw new InvalidOperationException("Account is not active");
            }
            // start db transaction
            using var dbTransaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // capture balance before deposit
                var balanceBefore = account.Balance;
                //Add the amount to the account balance
                var balanceAfter = account.Balance + request.Amount;
                account.Balance = balanceAfter;
                //Create a new transaction record
                var transaction = new Transaction
                {
                    Type = TransactionType.Deposit,
                    Amount = request.Amount,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow,
                    AccountId = request.AccountId
                };
                await _unitOfWork.Transactions.AddAsync(transaction);
                // create ledger entry for the transaction and save it to the database
                var ledgerEntry = new LedgerEntry
                {
                    TransactionId = transaction.Id,
                    AccountId = account.Id,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    Note = $"Deposit of {request.Amount:C} to account {account.AccountNumber}",
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LedgerEntries.AddAsync(ledgerEntry);
                // update the account balance in the database
                await _unitOfWork.Accounts.UpdateAsync(account);
                // commit transaction to db
                await dbTransaction.CommitAsync();
                // return the transaction response
                return new TransactionResponse
                {
                    Id = transaction.Id,
                    Type = transaction.Type,
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    CreatedAt = transaction.CreatedAt,
                    AccountId = transaction.AccountId
                };
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }


        /// <summary>
        /// Asynchronously retrieves all transactions associated with the specified account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account for which to retrieve transactions.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of transaction
        /// responses for the specified account. The collection is empty if the account has no transactions.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if an account with the specified accountId does not exist.</exception>
        public async Task<IEnumerable<TransactionResponse>> GetTransactionsByAccountIdAsync(int accountId)
        {
            // verify account exists
            var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new KeyNotFoundException("Account not found");
            }

            // pull transaction list
            var transactions = await _unitOfWork.Transactions.GetByAccountIdAsync(accountId);

            // return transaction response
            return transactions.Select(t => new TransactionResponse
            {
                Id = t.Id,
                Type = t.Type,
                Amount = t.Amount,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                AccountId = t.AccountId,
                RelatedAccountId = t.RelatedAccountId
            });
        }


        /// <summary>
        /// Transfers the specified amount from one account to another asynchronously, creating transaction and ledger
        /// records for both accounts.
        /// </summary>
        /// <remarks>Both the source and destination accounts must be active and distinct. The method
        /// updates account balances and persists transaction and ledger entries atomically. If any part of the
        /// operation fails, no changes are committed.</remarks>
        /// <param name="request">The transfer request containing the source and destination account IDs, the amount to transfer, and an
        /// optional description. The amount must be positive, and both accounts must exist and be active.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResponse for the
        /// source account transaction.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the source or destination account specified in the request does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if either account is not active, if the source and destination accounts are the same, or if the
        /// source account does not have sufficient balance.</exception>
        public async Task<TransactionResponse> TransferAsync(TransferRequest request)
        {
            // Get source account by id, verify it exists
            var fromAccount = await _unitOfWork.Accounts.GetByIdAsync(request.FromAccountId);
            if (fromAccount == null)
            {
                throw new KeyNotFoundException("Source account not found");
            }

            // Get destination account by id, verify it exists
            var toAccount = await _unitOfWork.Accounts.GetByIdAsync(request.ToAccountId);
            if (toAccount == null)
            {
                throw new KeyNotFoundException("Destination account not found");
            }

            // check if source account is active
            if (fromAccount.Status != AccountStatus.Active)
            {
                throw new InvalidOperationException("Source account is not active");
            }

            // check if destination account is active
            if (toAccount.Status != AccountStatus.Active)
            {
                throw new InvalidOperationException("Destination account is not active");
            }

            //check if source and destination accounts are the same
            if (fromAccount.Id == toAccount.Id)
            {
                throw new InvalidOperationException("Source and destination accounts cannot be the same");
            }

            // check if source account has sufficient balance
            if (fromAccount.Balance < request.Amount)
            {
                throw new InsufficientFundsException($"Insufficient funds. Current balance: {fromAccount.Balance:C}");
            }

            // start db transaction
            using var dbTransaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // capture balances before transfer
                var fromBalanceBefore = fromAccount.Balance;
                var toBalanceBefore = toAccount.Balance;
                // capture balances after transfer
                var fromBalanceAfter = fromAccount.Balance - request.Amount;
                var toBalanceAfter = toAccount.Balance + request.Amount;

                // create transfer transaction record for source account and save it to the database
                var fromTransaction = new Transaction
                {
                    Type = TransactionType.Transfer,
                    Amount = request.Amount,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow,
                    AccountId = fromAccount.Id,
                    RelatedAccountId = toAccount.Id
                };
                await _unitOfWork.Transactions.AddAsync(fromTransaction);

                // create transfer transaction record for destination account and save it to the database
                var toTransaction = new Transaction
                {
                    Type = TransactionType.Transfer,
                    Amount = request.Amount,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow,
                    AccountId = toAccount.Id,
                    RelatedAccountId = fromAccount.Id
                };
                await _unitOfWork.Transactions.AddAsync(toTransaction);

                // create ledger entry for source account transaction and save it to the database
                var fromLedgerEntry = new LedgerEntry
                {
                    TransactionId = fromTransaction.Id,
                    AccountId = fromAccount.Id,
                    BalanceBefore = fromBalanceBefore,
                    BalanceAfter = fromBalanceAfter,
                    Note = $"Transfer of {request.Amount:C} from account {fromAccount.AccountNumber} to account {toAccount.AccountNumber}",
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LedgerEntries.AddAsync(fromLedgerEntry);

                // create ledger entry for destination account transaction and save it to the database
                var toLedgerEntry = new LedgerEntry
                {
                    TransactionId = toTransaction.Id,
                    AccountId = toAccount.Id,
                    BalanceBefore = toBalanceBefore,
                    BalanceAfter = toBalanceAfter,
                    Note = $"Transfer of {request.Amount:C} from account {fromAccount.AccountNumber} to account {toAccount.AccountNumber}",
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LedgerEntries.AddAsync(toLedgerEntry);

                // update the account balances in the database
                fromAccount.Balance = fromBalanceAfter;
                toAccount.Balance = toBalanceAfter;
                await _unitOfWork.Accounts.UpdateAsync(fromAccount);
                await _unitOfWork.Accounts.UpdateAsync(toAccount);

                // commit transaction to db
                await dbTransaction.CommitAsync();

                // return the transaction response for the source account transaction
                return new TransactionResponse
                {
                    Id = fromTransaction.Id,
                    Type = fromTransaction.Type,
                    Amount = fromTransaction.Amount,
                    Description = fromTransaction.Description,
                    CreatedAt = fromTransaction.CreatedAt,
                    AccountId = fromTransaction.AccountId
                };
            }
            // Rollback db if anything fails
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }



        /// <summary>
        /// Processes a withdrawal from the specified account asynchronously and records the transaction.
        /// </summary>
        /// <remarks>The withdrawal operation is performed within a database transaction. If any part of
        /// the process fails, the transaction is rolled back and no changes are persisted.</remarks>
        /// <param name="request">The withdrawal request containing the account identifier, withdrawal amount, and an optional description.
        /// The account must be active and have sufficient balance. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResponse with
        /// details of the completed withdrawal transaction.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the account specified in the request does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the account is not active or does not have sufficient balance to complete the withdrawal.</exception>
        public async Task<TransactionResponse> WithdrawAsync(WithdrawRequest request)
        {
            // Get account by id, verify it exists
            var account = await _unitOfWork.Accounts.GetByIdAsync(request.AccountId);
            if (account == null)
            {
                throw new KeyNotFoundException("Account not found");
            }
            // check if account is active
            if (account.Status != AccountStatus.Active)
            {
                throw new InvalidOperationException("Account is not active");
            }
            // check if account has sufficient balance
            if (account.Balance < request.Amount)
            {
                throw new InsufficientFundsException($"Insufficient funds. Current balance: {account.Balance:C}");
            }

            // start db transaction
            using var dbTransaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // capture balance before withdrawal
                var balanceBefore = account.Balance;
                //capture balance after withdrawal
                var balanceAfter = account.Balance - request.Amount;
                account.Balance = balanceAfter;

                // create a new transaction record and save it to the database
                var transaction = new Transaction
                {
                    Type = TransactionType.Withdrawal,
                    Amount = request.Amount,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow,
                    AccountId = request.AccountId
                };
                await _unitOfWork.Transactions.AddAsync(transaction);

                // create ledger entry for the transaction and save it to the database
                var ledgerEntry = new LedgerEntry
                {
                    TransactionId = transaction.Id,
                    AccountId = account.Id,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    Note = $"Withdrawal of {request.Amount:C} from account {account.AccountNumber}",
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LedgerEntries.AddAsync(ledgerEntry);

                // update the account balance in the database
                await _unitOfWork.Accounts.UpdateAsync(account);

                // commit transaction to db
                await dbTransaction.CommitAsync();

                // return the transaction response
                return new TransactionResponse
                {
                    Id = transaction.Id,
                    Type = transaction.Type,
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    CreatedAt = transaction.CreatedAt,
                    AccountId = transaction.AccountId
                };
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }
    }
}
