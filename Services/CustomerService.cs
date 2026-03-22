using CommunityBankTellerAPI.DTOs;
using CommunityBankTellerAPI.Models;
using CommunityBankTellerAPI.Repositories.Interfaces;
using CommunityBankTellerAPI.Services.Interfaces;

namespace CommunityBankTellerAPI.Services
{
    public class CustomerService : ICustomerService
    {
        // Dependency on the database context for data access
        private readonly ICustomerRepository _customerRepository;

        // Constructor with dependency injection of the customer repository
        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }


        /// <summary>
        /// Asynchronously creates a new customer record using the specified request data.
        /// </summary>
        /// <param name="request">The details of the customer to create. Must not be null and must contain a unique email address.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a CustomerResponse object with
        /// the details of the newly created customer.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a customer with the specified email address already exists.</exception>
        public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request)
        {
            // check if email exists
            var existingCustomer = await _customerRepository.GetByEmailAsync(request.Email);
            if (existingCustomer != null)
            {
                throw new InvalidOperationException("A customer with this email already exists.");
            }

            // Map request DTO to Customer Model
            var customer = new Customer
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                Street = request.Street,
                City = request.City,
                State = request.State,
                ZipCode = request.ZipCode
            };

            // Save to Database
            await _customerRepository.AddAsync(customer);

            // Map to Response DTO and return
            return new CustomerResponse
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                Street = customer.Street,
                City = customer.City,
                State = customer.State,
                ZipCode = customer.ZipCode,
                CreatedAt = customer.CreatedAt
            };
        }

        /// <summary>
        /// Asynchronously retrieves a customer by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the customer to retrieve. Must be a valid customer ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="CustomerResponse"/>
        /// object with the customer's details.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if a customer with the specified <paramref name="id"/> does not exist.</exception>
        public async Task<CustomerResponse> GetCustomerByIdAsync(int id)
        {
            // Validate that the customer exists
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                throw new KeyNotFoundException("Customer not found.");
            }

            // Map to Response DTO and return
            return new CustomerResponse
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                Street = customer.Street,
                City = customer.City,
                State = customer.State,
                ZipCode = customer.ZipCode,
                CreatedAt = customer.CreatedAt
            };
        }


        /// <summary>
        /// Asynchronously updates the details of an existing customer with the specified values.
        /// </summary>
        /// <remarks>Fields in the request that are null or empty are ignored and will not overwrite
        /// existing customer data.</remarks>
        /// <param name="id">The unique identifier of the customer to update.</param>
        /// <param name="request">An object containing the updated customer information. Only non-null and non-empty fields will be applied.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a CustomerResponse object with
        /// the updated customer details.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if a customer with the specified id does not exist.</exception>
        public async Task<CustomerResponse> UpdateCustomerAsync(int id, UpdateCustomerRequest request)
        {
            // validate that the customer exists
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                throw new KeyNotFoundException("Customer not found.");
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.FirstName))
                customer.FirstName = request.FirstName;
            if (!string.IsNullOrEmpty(request.LastName))
                customer.LastName = request.LastName;
            if (!string.IsNullOrEmpty(request.Email))
                customer.Email = request.Email;
            if (!string.IsNullOrEmpty(request.Phone))
                customer.Phone = request.Phone;
            if (!string.IsNullOrEmpty(request.Street))
                customer.Street = request.Street;
            if (!string.IsNullOrEmpty(request.City))
                customer.City = request.City;
            if (!string.IsNullOrEmpty(request.State))
                customer.State = request.State;
            if (!string.IsNullOrEmpty(request.ZipCode))
                customer.ZipCode = request.ZipCode;

            // Update the UpdatedAt timestamp
            customer.UpdatedAt = DateTime.UtcNow;

            // Save changes to database
            await _customerRepository.UpdateAsync(customer);

            // Map to Response DTO and return
            return new CustomerResponse
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                Street = customer.Street,
                City = customer.City,
                State = customer.State,
                ZipCode = customer.ZipCode,
                CreatedAt = customer.CreatedAt
            };
        }
    }
}
