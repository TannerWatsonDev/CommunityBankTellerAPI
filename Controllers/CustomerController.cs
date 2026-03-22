using CommunityBankTellerAPI.DTOs;
using CommunityBankTellerAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommunityBankTellerAPI.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IAccountService _accountService;

        public CustomerController(ICustomerService customerService, IAccountService accountService)
        {
            _customerService = customerService;
            _accountService = accountService;
        }


        /// <summary>
        /// Creates a new customer using the specified request data.
        /// </summary>
        /// <remarks>If the customer is created successfully, the response includes the location of the
        /// new customer resource. If a customer with the specified email already exists, a conflict response is
        /// returned.</remarks>
        /// <param name="request">The details of the customer to create. Must not be null.</param>
        /// <returns>A 201 Created response containing the newly created customer's information if successful; otherwise, a 409
        /// Conflict response if a customer with the same email already exists.</returns>
        [HttpPost]
        [EndpointName("CreateCustomer")]
        public async Task<ActionResult<CustomerResponse>> CreateCustomer(CreateCustomerRequest request)
        {
            try
            {
                // Call the service and pass the request to create a new customer
                var response = await _customerService.CreateCustomerAsync(request);

                // Return a 201 Created response with the location of the new customer
                return CreatedAtAction(nameof(GetCustomerById), new { id = response.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                // return a 409 Conflict response if the email already exists
                return Conflict(ex.Message);
            }
        }


        /// <summary>
        /// Retrieves the customer with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the customer to retrieve.</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing a <see cref="CustomerResponse"/> if the customer is found;
        /// otherwise, a 404 Not Found response if no customer with the specified identifier exists.</returns>
        [HttpGet("{id}")]
        [EndpointName("GetCustomerById")]
        public async Task<ActionResult<CustomerResponse>> GetCustomerById(int id)
        {
            try
            {
                // Call the service to get the customer by ID and store in a variable
                var response = await _customerService.GetCustomerByIdAsync(id);
                // Return a 200 OK response with the customer data
                return Ok(response);

            }
            catch (KeyNotFoundException ex)
            {
                // Return a 404 Not Found response if the customer with the specified ID does not exist
                return NotFound(ex.Message);
            }

        }


        /// <summary>
        /// Updates the details of an existing customer with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the customer to update.</param>
        /// <param name="request">An object containing the updated customer information. Cannot be null.</param>
        /// <returns>An ActionResult containing the updated customer data if the update is successful; otherwise, a 404 Not Found
        /// response if the customer does not exist.</returns>
        [HttpPut("{id}")]
        [EndpointName("UpdateCustomer")]
        public async Task<ActionResult<CustomerResponse>> UpdateCustomer(int id, UpdateCustomerRequest request)
        {
            try
            {
                // Call the service to update the customer by ID and store in a variable
                var response = await _customerService.UpdateCustomerAsync(id, request);
                // Return a 200 OK response with the updated customer data
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                // Return a 404 Not Found response if the customer with the specified ID does not exist
                return NotFound(ex.Message);
            }
        }


        [HttpGet("{id}/accounts")]
        [EndpointName("GetAccountsByCustomerId")]
        public async Task<ActionResult<IEnumerable<AccountResponse>>> GetAccountsByCustomerId(int id)
        {
            try
            {
                // Create a variable to hold the list of accounts returned by the service
                var response = await _accountService.GetAccountsByCustomerIdAsync(id);
                // Return a 200 OK response with the list of accounts
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                // Return a 404 Not Found response if the customer with the specified ID does not exist
                return NotFound(ex.Message);
            }
        }
    }
}