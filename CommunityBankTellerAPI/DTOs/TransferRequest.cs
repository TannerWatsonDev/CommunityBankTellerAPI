using System.ComponentModel.DataAnnotations;

namespace CommunityBankTellerAPI.DTOs
{
    public class TransferRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        [Required]
        public int FromAccountId { get; set; }
        [Required]
        public int ToAccountId { get; set; }
    }
}
