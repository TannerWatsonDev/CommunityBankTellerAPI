using CommunityBankTellerAPI.DTOs;
using CommunityBankTellerAPI.Models;
using CommunityBankTellerAPI.Repositories.Interfaces;
using CommunityBankTellerAPI.Services;
using Moq;

namespace CommunityBankTellerAPI.Tests.Unit
{
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _mockCustomerRepository;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _customerService = new CustomerService(_mockCustomerRepository.Object);
        }

        [Fact]
        public async Task CreateCustomerAsync_DuplicateEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            // set up existing customer with same email
            var existingCustomer = new Customer
            {
                Id = 1,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@example.com",
                Phone = "555-555-5555",
                Street = "123 Main St",
                City = "Springfield",
                State = "MO",
                ZipCode = "65801"
            };

            // tell the mock repo to return the existing customer when searched by email
            _mockCustomerRepository
                .Setup(r => r.GetByEmailAsync("jane@example.com"))
                .ReturnsAsync(existingCustomer);

            // set up a new request with the same email
            var request = new CreateCustomerRequest
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "jane@example.com",
                Phone = "555-123-4567",
                Street = "456 Oak Ave",
                City = "Springfield",
                State = "MO",
                ZipCode = "65801"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _customerService.CreateCustomerAsync(request));
        }
    }
}
