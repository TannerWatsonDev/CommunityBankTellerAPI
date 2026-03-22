using CommunityBankTellerAPI.Data;
using CommunityBankTellerAPI.Models;
using CommunityBankTellerAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommunityBankTellerAPI.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        // Create database context
        private readonly AppDbContext _context;

        // Constructor to inject database context
        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Customer> AddAsync(Customer customer)
        {
            // Add new customer to the database
            _context.Customers.Add(customer);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return customer;
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            // Retrieve a customer by their email address
            return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            // Retrieve a customer by their ID
            return await _context.Customers.FindAsync(id);
        }

        public async Task<Customer> UpdateAsync(Customer customer)
        {
            // Update an existing customer in the database
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
            return customer;
        }
    }
}
