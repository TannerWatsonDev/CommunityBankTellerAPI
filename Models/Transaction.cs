namespace CommunityBankTellerAPI.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public enum Type 
        {
            Deposit,
            Withdrawal,
            Transfer
        }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AccountId { get; set; }
        public int? RelatedAccountId { get; set; }
    }
}
