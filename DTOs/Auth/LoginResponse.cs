namespace AceBackend.DTOs.Auth
{
    public class LoginResponse
    {
        public bool success { get; set; }
        public string token { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
    }
}
