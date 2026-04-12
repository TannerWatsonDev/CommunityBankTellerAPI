using System.ComponentModel.DataAnnotations;

namespace CommunityBankTellerAPI.DTOs
{
    public class WithdrawRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        [Required]
        public int AccountId { get; set; }
    }
}
