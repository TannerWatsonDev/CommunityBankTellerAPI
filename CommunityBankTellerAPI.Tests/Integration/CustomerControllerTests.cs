using System.Net;
using System.Net.Http.Json;
using CommunityBankTellerAPI.DTOs;

namespace CommunityBankTellerAPI.Tests.Integration
{
    [Collection("Integration Tests")]
    public class CustomerControllerTests : IntegrationTestBase
    {
        public CustomerControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task CreateCustomer_ValidRequest_Returns201WithCustomerData()
        {
            // Arrange
            // generate a unique email address to avoid conflicts with existing customers in the database
            var uniqueEmail = $"tanner_{Guid.NewGuid()}@test.com";
            // create a request object with all required fields filled out
            var request = new CreateCustomerRequest
            {
                FirstName = "Tanner",
                LastName = "Watson",
                Email = uniqueEmail,
                Phone = "417-522-5807",
                Street = "208 N Wynwood Ave",
                City = "Republic",
                State = "MO",
                ZipCode = "65738"
            };

            // Act
            // send a real HTTP POST request to /api/customers with the request object serialized as JSON
            var response = await Client.PostAsJsonAsync("/api/customers", request);

            // Assert
            // verify the API returned a 201 Created status code
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            // deserialize the JSON response body back into a CustomerResponse object
            var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
            // verify the response body is not null
            Assert.NotNull(customer);
            // verify the returned customer has the correct first name
            Assert.Equal("Tanner", customer.FirstName);
            // verify the returned customer has the correct email
            Assert.Equal(uniqueEmail, customer.Email);
            // verify the database assigned an ID
            Assert.True(customer.Id > 0);
        }

        [Fact]
        public async Task CreateCustomer_DuplicateEmail_Returns409()
        {
            // Arrange
            var uniqueEmail = $"tanner_{Guid.NewGuid()}@test.com";
            // create a request object with all required fields filled out
            var request = new CreateCustomerRequest
            {
                FirstName = "Tanner",
                LastName = "Watson",
                Email = uniqueEmail,
                Phone = "417-522-5807",
                Street = "289 n road ave",
                City = "orlando",
                State = "FL",
                ZipCode = "89756"
            };

            // Act
            // first request creates the customer
            await Client.PostAsJsonAsync("/api/customers", request);
            // second request with same email should return 409
            var response = await Client.PostAsJsonAsync("/api/customers", request);

            // Assert
            // verify the API returned a 409 status code
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task GetCustomerById_ValidId_Returns200()
        {
            // Arrange
            // generate a unique email address to avoid conflicts with existing customers in the database
            var uniqueEmail = $"tanner_{Guid.NewGuid()}@test.com";
            // create a request object with all required fields filled out
            var request = new CreateCustomerRequest
            {
                FirstName = "Tanner",
                LastName = "Watson",
                Email = uniqueEmail,
                Phone = "417-522-5807",
                Street = "208 N Wynwood Ave",
                City = "Republic",
                State = "MO",
                ZipCode = "65738"
            };

            // create the customer first to get a valid Id
            var createResponse = await Client.PostAsJsonAsync("/api/customers", request);
            // store the created customer's data for get by Id request
            var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);

            // Act
            // use the Id from created customer to GET the customer
            var response = await Client.GetAsync($"/api/customers/{createdCustomer!.Id}");

            // Assert
            // verify the API returned a 200 OK status code
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // deserialize the JSON response body back into a CustomerResponse object
            var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
            // verify the response body is not null
            Assert.NotNull(customer);
            // verify the returned customer has the correct first name
            Assert.Equal("Tanner", customer.FirstName);
            // verify the returned customer has the correct email
            Assert.Equal(uniqueEmail, customer.Email);
            // verify the database assigned an ID
            Assert.True(customer.Id > 0);
        }

        [Fact]
        public async Task GetCustomerById_InvalidId_Returns404()
        {
            // Arrange
            var invalidId = 9999;

            // Act
            // use the invalid Id from above to GET the customer
            var response = await Client.GetAsync($"/api/customers/{invalidId}");

            // Assert
            // verify the API returned a 404 NotFound status code
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}