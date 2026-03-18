namespace CommunityBankTellerAPI.Models
{
    public class Account
    {
        public int Id { get; set; }
        public required string AccountNumber { get; set; }
        public enum Types
        {
            Checking,
            Savings,
            Credit
        }
        public enum Status
        {
            Active,
            Closed,
            Frozen
        }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int CustomerId { get; set; }

    }
}
