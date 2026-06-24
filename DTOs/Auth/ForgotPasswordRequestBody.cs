using System.ComponentModel.DataAnnotations;

namespace AceBackend.DTOs.Auth
{
    public class ForgotPasswordRequestBody
    {
        [Required]
        [EmailAddress]
        public string identifier { get; set; } = string.Empty;
    }
}
