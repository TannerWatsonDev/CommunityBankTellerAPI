namespace CommunityBankTellerAPI.Models
{
    public class LedgerEntry
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int TransactionId { get; set; }
        public decimal BalanceBefore
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
