namespace CommunityBankTellerAPI.Models
{
    public class LedgerEntry
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int TransactionId { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public required string Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Account Account { get; set; } = null!;
        public Transaction Transaction { get; set; } = null!;
    }
}
