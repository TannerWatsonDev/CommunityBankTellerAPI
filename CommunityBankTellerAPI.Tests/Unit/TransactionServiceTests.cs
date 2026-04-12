using CommunityBankTellerAPI.DTOs;
using CommunityBankTellerAPI.Exceptions;
using CommunityBankTellerAPI.Models;
using CommunityBankTellerAPI.Repositories.Interfaces;
using CommunityBankTellerAPI.Services;
using Moq;

namespace CommunityBankTellerAPI.Tests.Unit
{
    public class TransactionServiceTests
    {
        // set up mock of unit of work
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        // set up mock of account repo
        private readonly Mock<IAccountRepository> _mockAccountRepository;
        // set up variable for Transaction Service
        private readonly TransactionService _transactionService;
        // set up variable for mock transaction repo
        private readonly Mock<ITransactionRepository> _mockTransactionRepository;
        // set up variable for mock ledger repo
        private readonly Mock<ILedgerRepository> _mockLedgerRepository;

        // constructor to inject unit of work, account repo, and transaction service into test file
        public TransactionServiceTests()
        {
            // create a fake IUnitOfWork
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            // create fake IAccountRepository
            _mockAccountRepository = new Mock<IAccountRepository>();
            //create fake ITransactionRepository
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            //create fake ILedgerRepositroy
            _mockLedgerRepository = new Mock<ILedgerRepository>();
            // setup the fake unit of work to return the fake account repo if .Accounts is called
            _mockUnitOfWork.Setup(u => u.Accounts).Returns(_mockAccountRepository.Object);
            // set up fake unit of work repo to return the fake transaction repo if transaction is called
            _mockUnitOfWork.Setup(u => u.Transactions).Returns(_mockTransactionRepository.Object);
            // set up fake unit of work repo to return the fake ledger repo if ledger is called
            _mockUnitOfWork.Setup(u => u.LedgerEntries).Returns(_mockLedgerRepository.Object);
            // set up unit of work to call fake begin transaction async for db transaction mock
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>().Object);
            // create real instance of transaction service but injects the fake unit of work into it
            _transactionService = new TransactionService(_mockUnitOfWork.Object);
        }

        // WithdrawAsync Test :Insufficient Funds Failure
        [Theory]
        [InlineData(50, 100)]
        [InlineData(0, 1)]
        [InlineData(10, 10.01)]
        public async Task WithdrawAsync_InsufficientFunds_ThrowsInsufficientFundsException(decimal balance, decimal amount)
        {
            // Arrange: set up a new Account object to test on
            var account = new Account
            {
                Id = 1,
                AccountNumber = "ACC00000001",
                Status = AccountStatus.Active,
                Balance = balance
            };

            // tell the mock account repo that when GetByIdAsync(1) is called,
            // return the fake account above instead of hitting a real DB
            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(account);

            // set up new instance of a Withdraw Request
            var request = new WithdrawRequest
            {
                AccountId = 1,
                Amount = amount
            };

            // Act & Assert
            // ThrowsAsync tests that the passed method "WithdrawAsync" throws the specified exception, if not it fails the test.
            await Assert.ThrowsAsync<InsufficientFundsException>(() =>
                _transactionService.WithdrawAsync(request));
        }

        // WithdrawAsync Test: Account Not Found Failure
        [Fact]
        public async Task WithdrawAsync_AccountNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange set up mock to return null for any ID thats passed
            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Account?)null);

            // set up new instance of a Withdraw Request
            var request = new WithdrawRequest
            {
                AccountId = 1,
                Amount = 100m
            };

            // Act and Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _transactionService.WithdrawAsync(request));
        }

        // WithdrawAsync Test: Account Not Found Exception Failure
        [Fact]
        public async Task WithdrawAsync_AccountClosed_ThrowsInvalidOperationException()
        {
            // Arrange
            // set up new account that is closed
            var account = new Account
            {
                Id = 1,
                AccountNumber = "ACC00000001",
                Status = AccountStatus.Closed,
                Balance = 10m
            };

            // tell the mock account repo that when GetByIdAsync(1) is called,
            // return the fake account above instead of hitting a real DB
            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(account);

            // set up new instance of a Withdraw Request
            var request = new WithdrawRequest
            {
                AccountId = 1,
                Amount = 10m
            };

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _transactionService.WithdrawAsync(request));
        }

        // WithdrawAsync Test: Valid Withdrawal request success
        [Theory]
        [InlineData(100, 100)]
        [InlineData(10, 0.01)]
        [InlineData(999999.99, 500000.00)]
        public async Task WithdrawAsync_ValidRequest_UpdatesBalanceAndReturnsTransaction(decimal balance, decimal amount)
        {
            //Arrange
            // setup account 
            var account = new Account
            {
                Id = 1,
                AccountNumber = "ACC00000001",
                Status = AccountStatus.Active,
                Balance = balance
            };
            // set up the mocked account repo to return the account that is passed when GetByIdAsync is called so the service can find the account made in the test instead of hitting  a real DB
            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(account);
            // set the mocked transaction repo to return the transaction that is passed in when AddAsync is called so no error is thrown when the service tries to add a transaction
            _mockTransactionRepository
                .Setup(r => r.AddAsync(It.IsAny<Transaction>()))
                .ReturnsAsync((Transaction t) => t);

            // set the mocked ledger to return a complete task when AddAsync is called so no error is thrown when the service tries to add a ledger entry
            _mockLedgerRepository
                .Setup(r => r.AddAsync(It.IsAny<LedgerEntry>()))
                .Returns(Task.CompletedTask);
            // set the mock account repo to return the account that is passed in when UpdateAsync is called so no error is thrown when the service tries to update the account balance after the withdrawal
            _mockAccountRepository
                    .Setup(r => r.UpdateAsync(It.IsAny<Account>()))
                    .ReturnsAsync((Account a) => a);
            // initialize a withdraw request object with the proper account id and amount to withdraw
            var request = new WithdrawRequest
            {
                AccountId = 1,
                Amount = amount
            };

            // Act
            var result = await _transactionService.WithdrawAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TransactionType.Withdrawal, result.Type);
            Assert.Equal(amount, result.Amount);
            Assert.Equal(1, result.AccountId);
        }



        // DepositAsync Test: Account Not Found Failure
        [Fact]
        public async Task DepositAsync_AccountNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange set up mock to return null for any ID thats passed
            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Account?)null);

            // set up new instance of a Withdraw Request
            var request = new DepositRequest
            {
                AccountId = 1,
                Amount = 100m
            };

            // Act and Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _transactionService.DepositAsync(request));
        }

        // DepositAsync Test: Account Closed Failure
        [Fact]
        public async Task DepositAsync_AccountClosed_ThrowsInvalidOperationException()
        {
            // Arrange
            // set up new account that is closed
            var account = new Account
            {
                Id = 1,
                AccountNumber = "ACC00000001",
                Status = AccountStatus.Closed,
                Balance = 10m
            };

            // tell the mock account repo that when GetByIdAsync(1) is called,
            // return the fake account above instead of hitting a real DB
            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(account);

            // set up new instance of a Withdraw Request
            var request = new DepositRequest
            {
                AccountId = 1,
                Amount = 10m
            };

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _transactionService.DepositAsync(request));
        }

        // DepositAsync Test: Valid Request Success
        [Theory]
        [InlineData(100, 100)]
        [InlineData(10, 0.01)]
        [InlineData(999999.99, 500000.00)]
        public async Task DepositAsync_ValidRequest_UpdatesBalanceAndReturnsTransaction(decimal balance, decimal amount)
        {
            //Arrange
            // setup account 
            var account = new Account
            {
                Id = 1,
                AccountNumber = "ACC00000001",
                Status = AccountStatus.Active,
                Balance = balance
            };
            // set up the mocked account repo to return the account that is passed when GetByIdAsync is called so the service can find the account made in the test instead of hitting  a real DB
            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(account);
            // set the mocked transaction repo to return the transaction that is passed in when AddAsync is called so no error is thrown when the service tries to add a transaction
            _mockTransactionRepository
                .Setup(r => r.AddAsync(It.IsAny<Transaction>()))
                .ReturnsAsync((Transaction t) => t);

            // set the mocked ledger to return a complete task when AddAsync is called so no error is thrown when the service tries to add a ledger entry
            _mockLedgerRepository
                .Setup(r => r.AddAsync(It.IsAny<LedgerEntry>()))
                .Returns(Task.CompletedTask);
            // set the mock account repo to return the account that is passed in when UpdateAsync is called so no error is thrown when the service tries to update the account balance after the withdrawal
            _mockAccountRepository
                    .Setup(r => r.UpdateAsync(It.IsAny<Account>()))
                    .ReturnsAsync((Account a) => a);
            // initialize a withdraw request object with the proper account id and amount to withdraw
            var request = new DepositRequest
            {
                AccountId = 1,
                Amount = amount
            };

            // Act
            var result = await _transactionService.DepositAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TransactionType.Deposit, result.Type);
            Assert.Equal(amount, result.Amount);
            Assert.Equal(1, result.AccountId);
        }



        // TransferAsync Test: Source Account Not Found Failure
        [Fact]
        public async Task TransferAsync_SourceAccountNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange set up mock to return null for any ID thats passed
            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Account?)null);

            // set up new instance of a Withdraw Request
            var request = new TransferRequest
            {
                FromAccountId = 1,
                ToAccountId = 2,
                Amount = 100m
            };

            // Act and Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _transactionService.TransferAsync(request));
        }

        // TransferAsync Test: Destination Account Not Found Failure
        [Fact]
        public async Task TransferAsync_DestinationAccountNotFound_ThrowsKeyNotFoundException()
        {
            var fromAccount = new Account
            {
                Id = 1,
                AccountNumber = "ACC00000001",
                Status = AccountStatus.Active,
                Balance = 1000m
            };

            // Arrange set up mock to return the fromAccount for the source account and null for the destination account
            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(fromAccount);

            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync((Account?)null);

            // set up new instance of a Withdraw Request
            var request = new TransferRequest
            {
                FromAccountId = 1,
                ToAccountId = 2,
                Amount = 100m
            };

            // Act and Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _transactionService.TransferAsync(request));
        }

        // TransferAsync Test: Source Account Closed Failure
        [Fact]
        public async Task TransferAsync_SourceAccountClosed_ThrowsInvalidOperationException()
        {
            // Arrange
            // set up new account that is closed
            var fromAccount = new Account
            {
                Id = 1,
                AccountNumber = "ACC00000001",
                Status = AccountStatus.Closed,
                Balance = 10m
            };

            var toAccount = new Account
            {
                Id = 2,
                AccountNumber = "ACC00000002",
                Status = AccountStatus.Active,
                Balance = 10m
            };

            // tell the mock account repo that when GetByIdAsync(1) is called,
            // return the fake account above instead of hitting a real DB
            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(fromAccount);

            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(toAccount);

            // set up new instance of a Withdraw Request
            var request = new TransferRequest
            {
                FromAccountId = 1,
                ToAccountId = 2,
                Amount = 10m
            };

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _transactionService.TransferAsync(request));
        }

        // TransferAsync Test: Destination Account Closed Failure
        [Fact]
        public async Task TransferAsync_DestinationAccountClosed_ThrowsInvalidOperationException()
        {
            // Arrange
            // set up new account that is closed
            var fromAccount = new Account
            {
                Id = 1,
                AccountNumber = "ACC00000001",
                Status = AccountStatus.Active,
                Balance = 10m
            };

            var toAccount = new Account
            {
                Id = 2,
                AccountNumber = "ACC00000002",
                Status = AccountStatus.Closed,
                Balance = 10m
            };

            // tell the mock account repo that when GetByIdAsync(1) is called,
            // return the fake accounts above instead of hitting a real DB
            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(fromAccount);

            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(toAccount);

            // set up new instance of a Withdraw Request
            var request = new TransferRequest
            {
                FromAccountId = 1,
                ToAccountId = 2,
                Amount = 10m
            };

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _transactionService.TransferAsync(request));
        }

        // TransferAsync Test: Same Account Failure
        [Fact]
        public async Task TransferAsync_SameAccount_ThrowsInvalidOperationException()
        {
            // Arrange
            // set up new account that is closed
            var account = new Account
            {
                Id = 1,
                AccountNumber = "ACC00000001",
                Status = AccountStatus.Active,
                Balance = 10m
            };


            // tell the mock account repo that when GetByIdAsync(1) is called,
            // return the fake accounts above instead of hitting a real DB
            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(account);

            // set up new instance of a Withdraw Request
            var request = new TransferRequest
            {
                FromAccountId = 1,
                ToAccountId = 1,
                Amount = 10m
            };

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _transactionService.TransferAsync(request));
        }

        // TransferAsync Test: Insufficient Funds Failure
        [Theory]
        [InlineData(50, 1000, 100)]
        [InlineData(0, 500, 1)]
        [InlineData(10, 200, 10.01)]
        public async Task TransferAsync_InsufficientFunds_ThrowsInsufficientFundsException(decimal fromBalance, decimal toBalance, decimal amount)
        {
            // Arrange: set up new account objects for source and destination accounts
            var fromAccount = new Account
            {
                Id = 1,
                AccountNumber = "ACC00000001",
                Status = AccountStatus.Active,
                Balance = fromBalance
            };

            var toAccount = new Account
            {
                Id = 2,
                AccountNumber = "ACC00000002",
                Status = AccountStatus.Active,
                Balance = toBalance
            };

            // tell the mock account repo that when GetByIdAsync(1) is called,
            // return the fake account above instead of hitting a real DB
            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(fromAccount);

            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(toAccount);

            // set up new instance of a Withdraw Request
            var request = new TransferRequest
            {
                FromAccountId = 1,
                ToAccountId = 2,
                Amount = amount
            };

            // Act & Assert
            // ThrowsAsync tests that the passed method "WithdrawAsync" throws the specified exception, if not it fails the test.
            await Assert.ThrowsAsync<InsufficientFundsException>(() =>
                _transactionService.TransferAsync(request));
        }

        // TransferAsync Test: Valid Request Success
        [Theory]
        [InlineData(500, 1000, 100)]
        [InlineData(100, 500, 50)]
        [InlineData(999999.99, 200, 500000.00)]
        public async Task TransferAsync_ValidRequest_UpdatesBalancesAndReturnsTransaction(decimal fromBalance, decimal toBalance, decimal amount)
        {
            // Arrange: set up new account objects for source and destination accounts
            var fromAccount = new Account
            {
                Id = 1,
                AccountNumber = "ACC00000001",
                Status = AccountStatus.Active,
                Balance = fromBalance
            };

            var toAccount = new Account
            {
                Id = 2,
                AccountNumber = "ACC00000002",
                Status = AccountStatus.Active,
                Balance = toBalance
            };

            // tell the mock account repo that when GetByIdAsync(1) is called,
            // return the fake account above instead of hitting a real DB
            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(fromAccount);

            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(toAccount);

            // set up transaction repo to return the transaction that is passed in when AddAsync is called so no error is thrown when the service tries to add a transaction
            _mockTransactionRepository
                .Setup(r => r.AddAsync(It.IsAny<Transaction>()))
                 .ReturnsAsync((Transaction t) => t);
            // set up ledger repo to return a complete task when AddAsync is called so no error is thrown when the service tries to add a ledger entry
            _mockLedgerRepository
                .Setup(r => r.AddAsync(It.IsAny<LedgerEntry>()))
                .Returns(Task.CompletedTask);
            // set up mock account repo to return the account that is passed in when UpdateAsync is called so no error is thrown when the service tries to update the account balance after the withdrawal
            _mockAccountRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Account>()))
                .ReturnsAsync((Account a) => a);

            // set up new instance of a Withdraw Request
            var request = new TransferRequest
            {
                FromAccountId = 1,
                ToAccountId = 2,
                Amount = amount
            };

            // Act
            var result = await _transactionService.TransferAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TransactionType.Transfer, result.Type);
            Assert.Equal(amount, result.Amount);
            Assert.Equal(1, result.AccountId);
        }



    } // end of class
} // end of namespace
