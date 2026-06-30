using AceBackend.Database;
using AceBackend.DTOs.Auth;
using AceBackend.Helpers;
using AceBackend.Models;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AceBackend.Services
{
    public class AuthService
    {
        private readonly MongoDbContext _dbContext;
        private readonly OtpService _otpService;
        private readonly IConfiguration _configuration;
        private readonly LocalizationHelper _localization;

        public AuthService(MongoDbContext dbContext, OtpService otpService, IConfiguration configuration, LocalizationHelper localization)
        {
            _dbContext = dbContext;
            _otpService = otpService;
            _configuration = configuration;
            _localization = localization;
        }

        public async Task<(bool success, string? token, string message)> Login(LoginRequestBody request, string language = "en")
        {
            // Validate user type
            if (!IsValidUserType(request.userType))
            {
                return (false, null, _localization.Get("auth.register.invalidUserType", language));
            }

            // Find user
            var user = await _dbContext.Users
                .Find(u => u.Email == request.email && u.UserType == request.userType)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return (false, null, _localization.Get("auth.login.invalidCredentials", language));
            }

            // Check if verified
            if (!user.IsVerified)
            {
                return (false, null, _localization.Get("auth.login.accountNotVerified", language));
            }

            // Verify password
            if (!PasswordHelper.Verify(request.password, user.PasswordHash))
            {
                return (false, null, _localization.Get("auth.login.invalidCredentials", language));
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);
            return (true, token, _localization.Get("auth.login.success", language));
        }

        public async Task<(bool success, string message)> Register(RegisterRequestBody request, string language = "en")
        {
            // Validate user type
            if (!IsValidUserType(request.userType))
            {
                return (false, _localization.Get("auth.register.invalidUserType", language));
            }

            // Validate password
            if (!PasswordHelper.IsValid(request.password))
            {
                return (false, _localization.Get("auth.register.weakPassword", language));
            }

            // Check if email exists
            var existingEmail = await _dbContext.Users
                .Find(u => u.Email == request.email)
                .FirstOrDefaultAsync();

            if (existingEmail != null)
            {
                return (false, _localization.Get("auth.register.emailExists", language));
            }

            // Check if username exists
            var existingUsername = await _dbContext.Users
                .Find(u => u.Username == request.username)
                .FirstOrDefaultAsync();

            if (existingUsername != null)
            {
                return (false, _localization.Get("auth.register.usernameExists", language));
            }

            // Check if phone exists
            var fullPhone = request.countryCode + request.phone;
            var existingPhone = await _dbContext.Users
                .Find(u => u.PhoneNumber == request.phone && u.CountryCode == request.countryCode)
                .FirstOrDefaultAsync();

            if (existingPhone != null)
            {
                return (false, _localization.Get("auth.register.phoneExists", language));
            }

            // Create user
            var user = new User
            {
                FirstName = request.firstName,
                SecondName = request.secondName,
                FamilyName = request.familyName,
                Username = request.username,
                Email = request.email,
                PhoneNumber = request.phone,
                CountryCode = request.countryCode,
                PasswordHash = PasswordHelper.Hash(request.password),
                UserType = request.userType,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.Users.InsertOneAsync(user);

            // Generate and send OTP
            await _otpService.GenerateAndSend(request.email, "registration", language);

            return (true, _localization.Get("auth.register.success", language));
        }

        public async Task<(bool success, string message)> VerifyOtp(VerifyOtpRequestBody request, string language = "en")
        {
            var (isValid, error) = await _otpService.VerifyOtp(request.identifier, request.otp, "registration");

            if (!isValid)
            {
                return (false, _localization.Get($"auth.otp.{error}", language));
            }

            // Mark user as verified
            var user = await _dbContext.Users
                .Find(u => u.Email == request.identifier)
                .FirstOrDefaultAsync();

            if (user != null)
            {
                user.IsVerified = true;
                user.UpdatedAt = DateTime.UtcNow;
                await _dbContext.Users.ReplaceOneAsync(u => u.Id == user.Id, user);
            }

            return (true, _localization.Get("auth.otp.verifySuccess", language));
        }

        public async Task<(bool success, string message)> ResendOtp(ResendOtpRequestBody request, string language = "en")
        {
            // Check cooldown
            if (!await _otpService.CanResend(request.identifier))
            {
                return (false, _localization.Get("auth.otp.resendCooldown", language));
            }

            // Check if user exists
            var user = await _dbContext.Users
                .Find(u => u.Email == request.identifier)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return (false, _localization.Get("auth.login.accountNotFound", language));
            }

            // Generate and send new OTP
            await _otpService.GenerateAndSend(request.identifier, "registration", language);

            return (true, _localization.Get("auth.otp.resendSuccess", language));
        }

        public async Task<(bool success, string message)> ForgotPassword(ForgotPasswordRequestBody request, string language = "en")
        {
            // Check if user exists
            var user = await _dbContext.Users
                .Find(u => u.Email == request.identifier)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return (false, _localization.Get("auth.forgotPassword.accountNotFound", language));
            }

            // Generate and send OTP
            await _otpService.GenerateAndSend(request.identifier, "passwordReset", language);

            return (true, _localization.Get("auth.forgotPassword.success", language));
        }

        public async Task<(bool success, string message)> VerifyPasswordResetOtp(VerifyOtpRequestBody request, string language = "en")
        {
            var (isValid, error) = await _otpService.VerifyOtp(request.identifier, request.otp, "passwordReset");

            if (!isValid)
            {
                return (false, _localization.Get($"auth.otp.{error}", language));
            }

            return (true, _localization.Get("auth.otp.verifySuccess", language));
        }

        public async Task<(bool success, string message)> ChangePassword(ChangePasswordRequestBody request, string language = "en")
        {
            // Validate password match
            if (request.password != request.passwordConfirmation)
            {
                return (false, _localization.Get("auth.changePassword.passwordMismatch", language));
            }

            // Validate password strength
            if (!PasswordHelper.IsValid(request.password))
            {
                return (false, _localization.Get("auth.changePassword.weakPassword", language));
            }

            // Check if OTP was verified
            if (!await _otpService.IsVerified(request.identifier, "passwordReset"))
            {
                return (false, _localization.Get("auth.changePassword.verificationRequired", language));
            }

            // Find user
            var user = await _dbContext.Users
                .Find(u => u.Email == request.identifier)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return (false, _localization.Get("auth.forgotPassword.accountNotFound", language));
            }

            // Update password
            user.PasswordHash = PasswordHelper.Hash(request.password);
            user.UpdatedAt = DateTime.UtcNow;
            await _dbContext.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

            return (true, _localization.Get("auth.changePassword.success", language));
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = new List<Claim>
            {
                new Claim("id", user.Id.ToString()),
                new Claim("phone", user.PhoneNumber)
            };

            Console.WriteLine("Storing user ID in JWT: " + user.Id); // Debugging line

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(270), // ~9 months
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private bool IsValidUserType(string userType)
        {
            var validTypes = new[] { "owner", "buyer", "broker" };
            return validTypes.Contains(userType.ToLower());
        }
    }
}
