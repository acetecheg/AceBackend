using System.ComponentModel.DataAnnotations;

namespace AceBackend.DTOs.Auth
{
    public class LoginRequestBody
    {
        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

        [Required]
        public string password { get; set; } = string.Empty;

        [Required]
        public string userType { get; set; } = string.Empty;
    }
}
