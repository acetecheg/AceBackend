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

                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                // Log the error but don't throw to prevent exposing email configuration issues
                Console.WriteLine($"Email sending failed: {ex.Message}");
                // In production, you might want to log to a proper logging service
            }
        }
    }
}
