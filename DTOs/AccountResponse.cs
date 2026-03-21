using CommunityBankTellerAPI.Models;

namespace CommunityBankTellerAPI.DTOs
{
    public class AccountResponse
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public AccountType Type { get; set; }
        public AccountStatus Status { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int CustomerId { get; set; }
    }
}