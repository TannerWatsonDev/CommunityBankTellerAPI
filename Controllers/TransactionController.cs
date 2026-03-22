using CommunityBankTellerAPI.DTOs;
using CommunityBankTellerAPI.Exceptions;
using CommunityBankTellerAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommunityBankTellerAPI.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase
    {
        // declare a private readonly field for transaction service
        private readonly ITransactionService _transactionService;

        // constructor
        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// Processes a deposit transaction for the specified account.
        /// </summary>
        /// <param name="request">The deposit request containing account information and the amount to deposit. Cannot be null.</param>
        /// <returns>An ActionResult containing a TransactionResponse with details of the completed deposit if successful;
        /// returns a 404 Not Found if the account is not found, or a 400 Bad Request if the deposit cannot be
        /// completed.</returns>
        [HttpPost("deposit")]
        [EndpointName("Deposit")]
        public async Task<ActionResult<TransactionResponse>> Deposit(DepositRequest request)
        {
            try
            {
                // call service and pass request
                var response = await _transactionService.DepositAsync(request);
                // return 200 if request works
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// Processes a withdrawal request and returns the result of the transaction.
        /// </summary>
        /// <param name="request">The withdrawal request containing account and amount details. Cannot be null.</param>
        /// <returns>An ActionResult containing the transaction response if the withdrawal is successful; returns a 404 Not Found
        /// if the account is not found, or a 400 Bad Request if the withdrawal cannot be completed.</returns>
        [HttpPost("withdraw")]
        [EndpointName("Withdraw")]
        public async Task<ActionResult<TransactionResponse>> Withdraw(WithdrawRequest request)
        {
            try
            {
                // call service and pass request
                var response = await _transactionService.WithdrawAsync(request);
                // return 200 if request works
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InsufficientFundsException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }


        /// <summary>
        /// Initiates a funds transfer based on the specified transfer request.
        /// </summary>
        /// <remarks>This endpoint processes a transfer between accounts as defined in the <paramref
        /// name="request"/>. The response includes transaction details if the transfer is successful. Ensure that all
        /// required fields in the request are provided and valid.</remarks>
        /// <param name="request">The details of the transfer to perform, including source and destination accounts, amount, and any
        /// additional transfer information. Cannot be null.</param>
        /// <returns>An <see cref="ActionResult{TransactionResponse}"/> containing the result of the transfer operation. Returns
        /// 200 OK with the transaction details if successful; 404 Not Found if the source or destination account is not
        /// found; or 400 Bad Request if the transfer cannot be completed due to invalid operation.</returns>
        [HttpPost("transfer")]
        [EndpointName("Transfer")]
        public async Task<ActionResult<TransactionResponse>> Transfer(TransferRequest request)
        {
            try
            {
                // call and pass request
                var response = await _transactionService.TransferAsync(request);
                // return 200 if request works
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InsufficientFundsException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }


        /// <summary>
        /// Retrieves all transactions associated with the specified account identifier.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account for which to retrieve transactions. Must be a valid, existing account
        /// ID.</param>
        /// <returns>An <see cref="ActionResult{T}">ActionResult</see> containing a collection of <see
        /// cref="TransactionResponse"/> objects for the specified account. Returns a 404 Not Found response if the
        /// account does not exist.</returns>
        [HttpGet("{accountId}")]
        [EndpointName("List Account Transactions")]
        public async Task<ActionResult<IEnumerable<TransactionResponse>>> GetTransactionsByAccountId(int accountId)
        {
            try
            {
                // call service to get transactions
                var response = await _transactionService.GetTransactionsByAccountIdAsync(accountId);
                // return 200 if request works
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
