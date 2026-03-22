using CommunityBankTellerAPI.Models;

namespace CommunityBankTellerAPI.DTOs
{
    public class TransactionResponse
    {
        public int Id { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AccountId { get; set; }
        public int? RelatedAccountId { get; set; }
    }
}
