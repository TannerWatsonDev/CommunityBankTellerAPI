using CommunityBankTellerAPI.DTOs;
using CommunityBankTellerAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommunityBankTellerAPI.Controllers
{
    [ApiController]
    [Route("api/ledger")]
    public class LedgerController : ControllerBase
    {
        // declare private field for ledger service
        private readonly ILedgerService _ledgerService;

        // constructor injecting ledger service into class
        public LedgerController(ILedgerService ledgerService)
        {
            _ledgerService = ledgerService;
        }


        [HttpGet("{accountId}")]
        public async Task<ActionResult<IEnumerable<LedgerEntryResponse>>> GetLedgerByAccountId(int accountId)
        {
            try
            {
                // call service to get the account by ID and store variable
                var response = await _ledgerService.GetLedgerByAccountIdAsync(accountId);
                // return 200 if response works with ledger entry info
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                // Return 404 Not Found
                return NotFound(ex.Message);
            }
        }

    }
}
