using System.ComponentModel.DataAnnotations;

namespace AceBackend.DTOs.Auth
{
    public class RegisterRequestBody
    {
        [Required]
        public string firstName { get; set; } = string.Empty;

        [Required]
        public string secondName { get; set; } = string.Empty;

        [Required]
        public string familyName { get; set; } = string.Empty;

        [Required]
        public string username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

        [Required]
        public string password { get; set; } = string.Empty;

        [Required]
        public string phone { get; set; } = string.Empty;

        [Required]
        public string countryCode { get; set; } = string.Empty;

        [Required]
        public string userType { get; set; } = string.Empty;
    }
}
