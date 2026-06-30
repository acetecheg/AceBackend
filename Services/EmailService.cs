using System.Net;
using System.Net.Mail;

namespace AceBackend.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpEmail(string toEmail, string otp, string language = "en")
        {
            var subject = language == "ar" ? "رمز التحقق الخاص بك" : "Your Verification Code";
            var body = language == "ar" 
                ? $"<h2>مرحباً</h2><p>رمز التحقق الخاص بك هو: <strong>{otp}</strong></p><p>هذا الرمز صالح لمدة 5 دقائق.</p>"
                : $"<h2>Hello</h2><p>Your verification code is: <strong>{otp}</strong></p><p>This code is valid for 5 minutes.</p>";

            await SendEmail(toEmail, subject, body);
        }

        private async Task SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration.GetValue<string>("Email:SmtpHost");
                var smtpPort = _configuration.GetValue<int>("Email:SmtpPort");
                var senderEmail = _configuration.GetValue<string>("Email:SenderEmail");
                var senderName = _configuration.GetValue<string>("Email:SenderName");
                var username = _configuration.GetValue<string>("Email:Username");
                var password = _configuration.GetValue<string>("Email:Password");

                Console.WriteLine($"=== EMAIL DEBUG INFO ===");
                Console.WriteLine($"SMTP Host: {smtpHost}");
                Console.WriteLine($"SMTP Port: {smtpPort}");
                Console.WriteLine($"Sender Email: {senderEmail}");
                Console.WriteLine($"Username: {username}");
                Console.WriteLine($"To Email: {toEmail}");
                Console.WriteLine($"Password configured: {!string.IsNullOrEmpty(password)}");
                Console.WriteLine($"========================");

                var message = new MailMessage
                {
                    From = new MailAddress(senderEmail ?? "", senderName ?? ""),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                Console.WriteLine("Attempting to send email...");
                await smtpClient.SendMailAsync(message);
                Console.WriteLine("✓ Email sent successfully!");
            }
            catch (Exception ex)
            {
                // Log the full error details
                Console.WriteLine($"✗ EMAIL SENDING FAILED!");
                Console.WriteLine($"Error Type: {ex.GetType().Name}");
                Console.WriteLine($"Error Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }
    }
}
