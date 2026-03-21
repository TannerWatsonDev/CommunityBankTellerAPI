namespace CommunityBankTellerAPI.Models
{
    public class Account
    {
        public int Id { get; set; }
        public required string AccountNumber { get; set; }
        public AccountType Type { get; set; }
        public AccountStatus Status { get; set; } = AccountStatus.Active;
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }
        public int CustomerId { get; set; }

        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    }
}
