namespace CommunityBankTellerAPI.DTOs
{
    public class LedgerEntryResponse
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int TransactionId { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Note { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
