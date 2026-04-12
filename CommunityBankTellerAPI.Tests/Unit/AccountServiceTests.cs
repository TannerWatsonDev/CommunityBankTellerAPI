using CommunityBankTellerAPI.DTOs;
using CommunityBankTellerAPI.Models;
using CommunityBankTellerAPI.Repositories.Interfaces;
using CommunityBankTellerAPI.Services;
using Moq;

namespace CommunityBankTellerAPI.Tests.Unit
{
    public class AccountServiceTests
    {
        private readonly Mock<IAccountRepository> _mockAccountRepository;
        private readonly Mock<ICustomerRepository> _mockCustomerRepository;
        private readonly AccountService _accountService;

        public AccountServiceTests()
        {
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _accountService = new AccountService(_mockAccountRepository.Object, _mockCustomerRepository.Object);
        }

        [Fact]
        public async Task CreateAccountAsync_CustomerNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            // tell the mock repo to return null when customer is searched by id
            _mockCustomerRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Customer?)null);

            var request = new CreateAccountRequest
            {
                CustomerId = 1,
                Type = AccountType.Checking
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _accountService.CreateAccountAsync(request));
        }

        [Fact]
        public async Task CloseAccountAsync_AccountAlreadyClosed_ThrowsInvalidOperationException()
        {
            // Arrange
            // set up an account that is already closed
            var account = new Account
            {
                Id = 1,
                AccountNumber = "ACC00000001",
                Status = AccountStatus.Closed,
                Balance = 0m,
                CustomerId = 1
            };

            // tell the mock repo to return the closed account
            _mockAccountRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(account);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _accountService.CloseAccountByIdAsync(1));
        }
    }
}