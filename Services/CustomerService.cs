using CommunityBankTellerAPI.Data;
using CommunityBankTellerAPI.DTOs;
using CommunityBankTellerAPI.Models;
using CommunityBankTellerAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommunityBankTellerAPI.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
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
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == request.Email);
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
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

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
        /// Asynchronously retrieves all accounts associated with the specified customer identifier.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer whose accounts are to be retrieved. Must correspond to an existing
        /// customer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of account
        /// responses for the specified customer. The collection is empty if the customer has no accounts.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no customer with the specified identifier exists.</exception>
        public async Task<IEnumerable<AccountResponse>> GetAccountsByCustomerIdAsync(int customerId)
        {
            // validate if customer exists
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == customerId);
            if (existingCustomer == null)
            {
                throw new KeyNotFoundException("A customer with this email already exists.");
            }

            // Retrieve accounts for the customer
            var accounts = await _context.Accounts
                .Where(a => a.CustomerId == customerId)
                .Select(a => new AccountResponse
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    Type = a.Type,
                    Status = a.Status,
                    Balance = a.Balance,
                    CreatedAt = a.CreatedAt,
                    ClosedAt = a.ClosedAt,
                    CustomerId = a.CustomerId
                })
                .ToListAsync();
            return accounts;
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
            var customer = await _context.Customers.FindAsync(id);
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
            var customer = await _context.Customers.FindAsync(id);
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
            await _context.SaveChangesAsync();

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
