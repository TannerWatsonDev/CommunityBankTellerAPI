using CommunityBankTellerAPI.DTOs;
using CommunityBankTellerAPI.Tests.Integration;
using System.Net;
using System.Net.Http.Json;

namespace CommunityBankTellerAPI.Tests.Integration
{
    [Collection("Integration Tests")]
    public class AccountControllerTests : IntegrationTestBase
    {
        public AccountControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task CreateAccount_ValidRequest_Returns201()
        {
            // Arrange
            // create customer first so account request can ref a valid customer Id
            var uniqueEmail = $"tanner_{Guid.NewGuid()}@test.com";
            var newCustomerRequest = new CreateCustomerRequest
            {
                FirstName = "Tanner",
                LastName = "Watson",
                Email = uniqueEmail,
                Phone = "417-522-5807",
                Street = "548 w street",
                City = "Orlando",
                State = "FL",
                ZipCode = "54682"
            };
            // create the customer and get the response
            var createCustomerResponse = await Client.PostAsJsonAsync("/api/customers", newCustomerRequest);
            // store the created customer's data for get by Id request
            var createdCustomer = await createCustomerResponse.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);

            // create a request object with all required fields filled out
            var createAccountRequest = new CreateAccountRequest
            {
                CustomerId = createdCustomer!.Id,
                Type = Models.AccountType.Checking
            };
            // Act
            // send request to the api and store the response
            var response = await Client.PostAsJsonAsync("/api/accounts", createAccountRequest);

            // Assert
            // verify the API returned a 201 Created status code
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            // deserialize the JSON response body back into a CustomerResponse object
            var account = await response.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
            // verify the response body is not null
            Assert.NotNull(account);
            // verify the customer id in reponse matches the one sent in by the request
            Assert.Equal(createAccountRequest.CustomerId, account.CustomerId);
            // verift the account type in resposne matches the one in the request
            Assert.Equal(createAccountRequest.Type, account.Type);
            // verify the account number is not null or empty
            Assert.False(string.IsNullOrEmpty(account.AccountNumber));
            // verify the account status is set to Active
            Assert.Equal(Models.AccountStatus.Active, account.Status);
        }

        [Fact]
        public async Task GetAccountById_ValidId_Returns200()
        {
            // Arrange
            // create customer first so account request can ref a valid customer Id
            var uniqueEmail = $"tanner_{Guid.NewGuid()}@test.com";
            var newCustomerRequest = new CreateCustomerRequest
            {
                FirstName = "Tanner",
                LastName = "Watson",
                Email = uniqueEmail,
                Phone = "417-522-5807",
                Street = "548 w street",
                City = "Orlando",
                State = "FL",
                ZipCode = "54682"
            };
            // create the customer and get the response
            var createCustomerResponse = await Client.PostAsJsonAsync("/api/customers", newCustomerRequest);
            // store the created customer's data for get by Id request
            var createdCustomer = await createCustomerResponse.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);

            // create a request object with all required fields filled out
            var createAccountRequest = new CreateAccountRequest
            {
                CustomerId = createdCustomer!.Id,
                Type = Models.AccountType.Checking
            };
            // send request to the api to create an account and store the response
            var createAccountResponse = await Client.PostAsJsonAsync("/api/accounts", createAccountRequest);
            // store the created accounts data for the get by Id request
            var createdAccount = await createAccountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);

            // Act
            // get the created account by id and store the response
            var response = await Client.GetAsync($"/api/accounts/{createdAccount!.Id}");

            // Assert
            // verify the API returned a 200 OK status code
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // deserialize the JSON response body back into a CustomerResponse object
            var account = await response.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
            // verify the response body is not null
            Assert.NotNull(account);
            // verify the account Id in response matches the one in the created account
            Assert.Equal(createdAccount.Id, account.Id);
            // verify the customer Id in response matches the one sent in by the request
            Assert.Equal(createAccountRequest.CustomerId, account.CustomerId);
            // verify the account type in the response matches the one in the request
            Assert.Equal(createdAccount.Type, account.Type);
            // verify the account number is not null or empty
            Assert.False(string.IsNullOrEmpty(account.AccountNumber));
            // verify the account status is set to Active
            Assert.Equal(Models.AccountStatus.Active, account.Status);
        }

        [Fact]
        public async Task GetAccountById_InvalidId_Returns404()
        {
            // arrange
            // create invalid account id that doesnt exist in the database
            var invalidId = 99999;

            // Act
            // use the invalid Id from above to GET the customer
            var response = await Client.GetAsync($"/api/accounts/{invalidId}");

            // Assert
            // verify the API returned a 404 NotFound status code
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CloseAccount_ValidRequest_Returns200()
        {
            // Arrange
            // create customer first so account request can ref a valid customer Id
            var uniqueEmail = $"tanner_{Guid.NewGuid()}@test.com";
            var newCustomerRequest = new CreateCustomerRequest
            {
                FirstName = "Tanner",
                LastName = "Watson",
                Email = uniqueEmail,
                Phone = "417-522-5807",
                Street = "548 w street",
                City = "Orlando",
                State = "FL",
                ZipCode = "54682"
            };
            // create the customer and get the response
            var createCustomerResponse = await Client.PostAsJsonAsync("/api/customers", newCustomerRequest);
            // store the created customer's data for get by Id request
            var createdCustomer = await createCustomerResponse.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);

            // create a request object with all required fields filled out
            var createAccountRequest = new CreateAccountRequest
            {
                CustomerId = createdCustomer!.Id,
                Type = Models.AccountType.Checking
            };
            // send request to the api to create an account and store the response
            var createAccountResponse = await Client.PostAsJsonAsync("/api/accounts", createAccountRequest);
            // store the created accounts data for the get by Id request
            var createdAccount = await createAccountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);

            // Act
            // send request to the api to close the account and store the response
            var response = await Client.DeleteAsync($"/api/accounts/{createdAccount!.Id}");

            // Assert
            // verify the API returned a 200 OK status code
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // deserialize the JSON response body back into a CustomerResponse object
            var account = await response.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
            // verify the response body is not null
            Assert.NotNull(account);
            // verify the account Id in response matches the one in the closed account
            Assert.Equal(createdAccount.Id, account.Id);
            // verify the customer Id in response matches the one sent in by the request
            Assert.Equal(createAccountRequest.CustomerId, account.CustomerId);
            // verify the account type in the response matches the one in the request
            Assert.Equal(createdAccount.Type, account.Type);
            // verify the account number is not null or empty
            Assert.False(string.IsNullOrEmpty(account.AccountNumber));
            // verify the account status is set to Closed
            Assert.Equal(Models.AccountStatus.Closed, account.Status);
        }

        [Fact]
        public async Task CloseAccount_AlreadyClosed_Returns400()
        {
            // Arrange
            // create customer first so account request can ref a valid customer Id
            var uniqueEmail = $"tanner_{Guid.NewGuid()}@test.com";
            var newCustomerRequest = new CreateCustomerRequest
            {
                FirstName = "Tanner",
                LastName = "Watson",
                Email = uniqueEmail,
                Phone = "417-522-5807",
                Street = "548 w street",
                City = "Orlando",
                State = "FL",
                ZipCode = "54682"
            };
            // create the customer and get the response
            var createCustomerResponse = await Client.PostAsJsonAsync("/api/customers", newCustomerRequest);
            // store the created customer's data for get by Id request
            var createdCustomer = await createCustomerResponse.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);

            // create a request object with all required fields filled out
            var createAccountRequest = new CreateAccountRequest
            {
                CustomerId = createdCustomer!.Id,
                Type = Models.AccountType.Checking
            };
            // send request to the api to create an account and store the response
            var createAccountResponse = await Client.PostAsJsonAsync("/api/accounts", createAccountRequest);
            // store the created accounts data for the get by Id request
            var createdAccount = await createAccountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);

            // send request to the api to close the account and store the response
            var closeAccount = await Client.DeleteAsync($"/api/accounts/{createdAccount!.Id}");

            // Act
            // try to close the account again and store the response
            var response = await Client.DeleteAsync($"/api/accounts/{createdAccount.Id}");

            // Assert
            // verify the API returned a 400 BadRequest status code since the account is already closed
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}