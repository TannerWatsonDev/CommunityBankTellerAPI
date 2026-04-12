using System.ComponentModel.DataAnnotations;

namespace CommunityBankTellerAPI.DTOs
{
    public class CreateCustomerRequest
    {
        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        [Phone]
        public required string Phone { get; set; }
        [Required]
        [StringLength(100)]
        public required string Street { get; set; }
        [Required]
        [StringLength(50)]
        public required string City { get; set; }
        [Required]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "State must be a 2-letter abbreviation.")]
        public required string State { get; set; }
        [Required]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Zip code must be exactly 5 digits.")]
        public required string ZipCode { get; set; }
    }
}
