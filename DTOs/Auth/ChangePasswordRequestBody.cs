using System.ComponentModel.DataAnnotations;

namespace AceBackend.DTOs.Auth
{
    public class ChangePasswordRequestBody
    {
        [Required]
        [EmailAddress]
        public string identifier { get; set; } = string.Empty;

        [Required]
        public string password { get; set; } = string.Empty;

        [Required]
        public string passwordConfirmation { get; set; } = string.Empty;
    }
}
