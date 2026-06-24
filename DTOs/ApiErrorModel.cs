namespace AceBackend.DTOs
{
    public class ApiErrorModel
    {
        public bool success { get; set; } = false;
        public List<string> errors { get; set; } = new List<string>();
    }
}
