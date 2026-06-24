using System.ComponentModel.DataAnnotations;

namespace AceBackend.DTOs.Auth
{
    public class VerifyOtpRequestBody
    {
        [Required]
        [EmailAddress]
        public string identifier { get; set; } = string.Empty;

        [Required]
        public string otp { get; set; } = string.Empty;
    }
}
