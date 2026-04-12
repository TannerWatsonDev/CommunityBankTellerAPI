namespace CommunityBankTellerAPI.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int AccountId { get; set; }
        public int? RelatedAccountId { get; set; }

        // Navigation properties
        public Account Account { get; set; } = null!;
        public Account? RelatedAccount { get; set; }
    }
}
