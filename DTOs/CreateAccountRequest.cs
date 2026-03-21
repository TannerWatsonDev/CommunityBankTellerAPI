using System.ComponentModel.DataAnnotations;
using CommunityBankTellerAPI.Models;

namespace CommunityBankTellerAPI.DTOs
{
    public class CreateAccountRequest
    {
        [Required]
        public int CustomerId { get; set; }
        [Required]
        public AccountType Type{ get; set; }
    }
}
