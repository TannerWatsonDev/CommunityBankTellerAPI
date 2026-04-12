using System.ComponentModel.DataAnnotations;

namespace CommunityBankTellerAPI.DTOs
{
    public class UpdateCustomerRequest
    {
        [StringLength(50)]
        public string? FirstName { get; set; }
        [StringLength(50)]
        public string? LastName { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        [Phone]
        public string? Phone { get; set; }
        [StringLength(100)]
        public string? Street { get; set; }
        [StringLength(50)]
        public string? City { get; set; }
        [StringLength(2, MinimumLength = 2, ErrorMessage = "State must be a 2-letter abbreviation.")]
        public string? State { get; set; }
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Zip code must be exactly 5 digits.")]
        public string? ZipCode { get; set; }
    }
}
