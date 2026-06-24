using System.ComponentModel.DataAnnotations;

namespace AceBackend.DTOs.Auth
{
    public class ResendOtpRequestBody
    {
        [Required]
        [EmailAddress]
        public string identifier { get; set; } = string.Empty;
    }
}
