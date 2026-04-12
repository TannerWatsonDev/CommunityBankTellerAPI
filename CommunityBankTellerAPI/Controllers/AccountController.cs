using CommunityBankTellerAPI.DTOs;
using CommunityBankTellerAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommunityBankTellerAPI.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountController : ControllerBase
    {
        // Declare a private readonly field for the account service
        private readonly IAccountService _accountService;

        // Constructor that takes an IAccountService and assigns it to the private field
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }


        /// <summary>
        /// Creates a new account using the specified account details.
        /// </summary>
        /// <param name="request">The account information to use when creating the new account. Cannot be null.</param>
        /// <returns>A 201 Created response containing the newly created account details if successful; otherwise, a 404 Not
        /// Found response if the associated customer does not exist.</returns>
        [HttpPost]
        [EndpointName("CreateAccount")]
        public async Task<ActionResult<AccountResponse>> CreateAccount(CreateAccountRequest request)
        {
            try
            {
                // Call the service to create a new account and store in a variable
                var response = await _accountService.CreateAccountAsync(request);
                // Return a 201 Created response with the location of the new account
                return CreatedAtAction(nameof(GetAccountById), new { id = response.Id }, response);
            }
            catch (KeyNotFoundException ex)
            {
                // Return a 404 Not Found response if the customer does not exist
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the account details for the specified account identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the account to retrieve.</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing the account details if found; otherwise, a 404 Not Found
        /// response if the account does not exist.</returns>
        [HttpGet("{id}")]
        [EndpointName("GetAccountById")]
        public async Task<ActionResult<AccountResponse>> GetAccountById(int id)
        {
            try
            {
                // Call the service to get the account by ID and store in a variable
                var response = await _accountService.GetAccountByIdAsync(id);
                // Return an OK response with the account information
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                // Return a 404 Not Found response if the account does not exist
                return NotFound(ex.Message);
            }
        }


        /// <summary>
        /// Closes the account with the specified identifier and returns the updated account information.
        /// </summary>
        /// <param name="id">The unique identifier of the account to close.</param>
        /// <returns>An ActionResult containing the updated account information if the operation succeeds. Returns a 404 Not
        /// Found response if the account does not exist, or a 400 Bad Request response if the account is already
        /// closed.</returns>
        [HttpDelete("{id}")]
        [EndpointName("CloseAccountById")]
        public async Task<ActionResult<AccountResponse>> CloseAccountById(int id)
        {
            try
            {
                // Call the service to close the account by ID and store in a variable
                var response = await _accountService.CloseAccountByIdAsync(id);
                // Return an OK response with the updated account information
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                // Return a 404 Not Found response if the account does not exist
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // Return a 400 Bad Request response if the account is already closed
                return BadRequest(ex.Message);
            }
        }
    }
}
