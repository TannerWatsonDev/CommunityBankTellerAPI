using CommunityBankTellerAPI.DTOs;
using CommunityBankTellerAPI.Models;
using CommunityBankTellerAPI.Tests.Integration;
using System.Net;
using System.Net.Http.Json;

namespace CommunityBankTellerAPI.Tests.Integration
{
    [Collection("Integration Tests")]
    public class TransactionControllerTests : IntegrationTestBase
    {
        public TransactionControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task Deposit_ValidRequest_Returns200()
        {
            // Arrange
            // create customer
            var createCustomerResponse = await Client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest
            {
                FirstName = "John",
                LastName = "Smith",
                Email = $"john_{Guid.NewGuid()}@test.com",
                Phone = "555-123-4567",
                Street = "123 Main St",
                City = "Kansas City",
                State = "MO",
                ZipCode = "64101"
            });
            var customer = await createCustomerResponse.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);

            // create account
            var createAccountResponse = await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest
            {
                CustomerId = customer!.Id,
                Type = Models.AccountType.Checking
            });
            // deserialize account response to get account ID for the deposit request
            var account = await createAccountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);

            // Act
            // make deposit request
            var response = await Client.PostAsJsonAsync("/api/transactions/deposit", new DepositRequest
            {
                AccountId = account!.Id,
                Amount = 100m
            });

            // Assert
            // verify response status code is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // deserialize response body to get transaction details
            var transaction = await response.Content.ReadFromJsonAsync<TransactionResponse>(JsonOptions);
            // verify transaction is not null
            Assert.NotNull(transaction);
            // verify transaction type is Deposit
            Assert.Equal(Models.TransactionType.Deposit, transaction.Type);
            // verify transaction amount matches the deposit amount in the request
            Assert.Equal(100.00m, transaction.Amount);
        }

        [Fact]
        public async Task Withdraw_ValidRequest_Returns200()
        {
            // Arrange
            // create customer
            var createCustomerResponse = await Client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest
            {
                FirstName = "John",
                LastName = "Smith",
                Email = $"john_{Guid.NewGuid()}@test.com",
                Phone = "555-123-4567",
                Street = "123 Main St",
                City = "Kansas City",
                State = "MO",
                ZipCode = "64101"
            });
            var customer = await createCustomerResponse.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);

            // create account
            var createAccountResponse = await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest
            {
                CustomerId = customer!.Id,
                Type = Models.AccountType.Checking
            });
            var account = await createAccountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);

            // deposit funds first so there is a balance to withdraw from
            // make deposit request
            var deposit = await Client.PostAsJsonAsync("/api/transactions/deposit", new DepositRequest
            {
                AccountId = account!.Id,
                Amount = 100m
            });

            // Act
            // make withdraw request
            var response = await Client.PostAsJsonAsync("/api/transactions/withdraw", new WithdrawRequest
            {
                AccountId = account.Id,
                Amount = 50m
            });

            // Assert
            // verify response status code is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            //deserialize response body to get transaction details
            var transaction = await response.Content.ReadFromJsonAsync<TransactionResponse>(JsonOptions);
            // verify transaction is not null
            Assert.NotNull(transaction);
            // verify transaction type is Withdraw
            Assert.Equal(Models.TransactionType.Withdrawal, transaction.Type);
            // verify transaction amount matches the withdraw amount in the request
            Assert.Equal(50.00m, transaction.Amount);
            // verify transaction account ID matches the account ID in the request
            Assert.Equal(account.Id, transaction.AccountId);
        }

        [Fact]
        public async Task Withdraw_InsufficientFunds_Returns422()
        {
            // Arrange
            // create customer
            var createCustomerResponse = await Client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest
            {
                FirstName = "John",
                LastName = "Smith",
                Email = $"john_{Guid.NewGuid()}@test.com",
                Phone = "555-123-4567",
                Street = "123 Main St",
                City = "Kansas City",
                State = "MO",
                ZipCode = "64101"
            });
            var customer = await createCustomerResponse.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);

            // create account
            var createAccountResponse = await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest
            {
                CustomerId = customer!.Id,
                Type = Models.AccountType.Checking
            });
            var account = await createAccountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);

            // deposit funds first so there is a balance to withdraw from
            // make deposit request
            var deposit = await Client.PostAsJsonAsync("/api/transactions/deposit", new DepositRequest
            {
                AccountId = account!.Id,
                Amount = 100m
            });

            // Act
            // make withdraw request
            var response = await Client.PostAsJsonAsync("/api/transactions/withdraw", new WithdrawRequest
            {
                AccountId = account.Id,
                Amount = 150m
            });

            // Assert
            // verify response status code is 422 Unprocessable Entity
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task Transfer_ValidRequest_Returns200()
        {
            // Arrange
            // create customer
            var createCustomerResponse = await Client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest
            {
                FirstName = "John",
                LastName = "Smith",
                Email = $"john_{Guid.NewGuid()}@test.com",
                Phone = "555-123-4567",
                Street = "123 Main St",
                City = "Kansas City",
                State = "MO",
                ZipCode = "64101"
            });
            var customer = await createCustomerResponse.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);

            // create source account
            var createFromAccountResponse = await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest
            {
                CustomerId = customer!.Id,
                Type = Models.AccountType.Checking
            });
            var fromAccount = await createFromAccountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);

            // create destination account
            var createToAccountResponse = await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest
            {
                CustomerId = customer.Id,
                Type = Models.AccountType.Savings
            });
            var toAccount = await createToAccountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);

            // deposit funds into source account so there is a balance to transfer
            // make deposit request
            var deposit = await Client.PostAsJsonAsync("/api/transactions/deposit", new DepositRequest
            {
                AccountId = fromAccount!.Id,
                Amount = 100m
            });

            // Act
            // make transfer request
            var response = await Client.PostAsJsonAsync("/api/transactions/transfer", new TransferRequest
            {
                FromAccountId = fromAccount.Id,
                ToAccountId = toAccount!.Id,
                Amount = 50m
            });

            // Assert
            // verify response status code is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // deserialize response body to get transaction details
            var transaction = await response.Content.ReadFromJsonAsync<TransactionResponse>(JsonOptions);
            // verify transaction is not null
            Assert.NotNull(transaction);
            // verify transaction type is Transfer
            Assert.Equal(Models.TransactionType.Transfer, transaction.Type);
            // verify transaction amount matches the transfer amount in the request
            Assert.Equal(50.00m, transaction.Amount);
            // verify transaction account ID matches the source account ID in the request
            Assert.Equal(fromAccount.Id, transaction.AccountId);
        }
    }
}