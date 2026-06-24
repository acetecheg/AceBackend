using AceBackend.Database;
using AceBackend.Helpers;
using AceBackend.Models;
using MongoDB.Driver;

namespace AceBackend.Services
{
    public class OtpService
    {
        private readonly MongoDbContext _dbContext;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;

        public OtpService(MongoDbContext dbContext, EmailService emailService, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<bool> CanResend(string identifier)
        {
            var cooldownSeconds = _configuration.GetValue<int>("Otp:ResendCooldownSeconds");
            var lastOtp = await _dbContext.OtpRecords
                .Find(o => o.Identifier == identifier && o.LastResendAt != null)
                .SortByDescending(o => o.LastResendAt)
                .FirstOrDefaultAsync();

            if (lastOtp == null) return true;

            var timeSinceLastResend = DateTime.UtcNow - (lastOtp.LastResendAt ?? lastOtp.CreatedAt);
            return timeSinceLastResend.TotalSeconds >= cooldownSeconds;
        }

        public async Task<OtpRecord> GenerateAndSend(string identifier, string purpose, string language = "en")
        {
            var otpLength = _configuration.GetValue<int>("Otp:Length");
            var expirationMinutes = _configuration.GetValue<int>("Otp:ExpirationMinutes");

            // Invalidate all previous OTPs for this identifier and purpose
            await _dbContext.OtpRecords.UpdateManyAsync(
                o => o.Identifier == identifier && o.Purpose == purpose && !o.IsVerified,
                Builders<OtpRecord>.Update.Set(o => o.ExpiresAt, DateTime.UtcNow.AddMinutes(-1))
            );

            var otp = OtpHelper.Generate(otpLength);
            var otpRecord = new OtpRecord
            {
                Identifier = identifier,
                Otp = otp,
                Purpose = purpose,
                Attempts = 0,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                LastResendAt = DateTime.UtcNow
            };

            await _dbContext.OtpRecords.InsertOneAsync(otpRecord);
            await _emailService.SendOtpEmail(identifier, otp, language);

            return otpRecord;
        }

        public async Task<(bool isValid, string? error)> VerifyOtp(string identifier, string otp, string purpose)
        {
            var maxAttempts = _configuration.GetValue<int>("Otp:MaxAttempts");

            var otpRecord = await _dbContext.OtpRecords
                .Find(o => o.Identifier == identifier && o.Purpose == purpose && !o.IsVerified)
                .SortByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
            {
                return (false, "invalidOtp");
            }

            if (DateTime.UtcNow > otpRecord.ExpiresAt)
            {
                return (false, "otpExpired");
            }

            if (otpRecord.Attempts >= maxAttempts)
            {
                return (false, "maxAttemptsReached");
            }

            // Increment attempts
            otpRecord.Attempts++;
            await _dbContext.OtpRecords.ReplaceOneAsync(
                o => o.Id == otpRecord.Id,
                otpRecord
            );

            if (otpRecord.Otp != otp)
            {
                return (false, "invalidOtp");
            }

            // Mark as verified
            otpRecord.IsVerified = true;
            await _dbContext.OtpRecords.ReplaceOneAsync(
                o => o.Id == otpRecord.Id,
                otpRecord
            );

            return (true, null);
        }

        public async Task<bool> IsVerified(string identifier, string purpose)
        {
            var verifiedOtp = await _dbContext.OtpRecords
                .Find(o => o.Identifier == identifier && o.Purpose == purpose && o.IsVerified)
                .SortByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            return verifiedOtp != null;
        }
    }
}
