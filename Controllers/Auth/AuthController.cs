using AceBackend.DTOs;
using AceBackend.DTOs.Auth;
using AceBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace AceBackend.Controllers.Auth
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        private string GetLanguage()
        {
            return HttpContext.Items["Language"]?.ToString() ?? "en";
        }

        private IActionResult Error(string message, int statusCode = 422)
        {
            return StatusCode(statusCode, new ApiErrorModel
            {
                success = false,
                errors = new List<string> { message }
            });
        }

        private IActionResult Errors(List<string> messages, int statusCode = 422)
        {
            return StatusCode(statusCode, new ApiErrorModel
            {
                success = false,
                errors = messages
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestBody request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Errors(errors);
            }

            var language = GetLanguage();
            var (success, token, message) = await _authService.Login(request, language);

            if (!success)
            {
                return Error(message, 401);
            }

            return Ok(new LoginResponse
            {
                success = true,
                token = token ?? string.Empty,
                message = message
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestBody request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Errors(errors);
            }

            var language = GetLanguage();
            var (success, message) = await _authService.Register(request, language);

            if (!success)
            {
                return Error(message);
            }

            return Ok(new AuthResponse
            {
                success = true,
                token = null,
                message = message
            });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestBody request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Errors(errors);
            }

            var language = GetLanguage();
            var (success, message) = await _authService.VerifyOtp(request, language);

            if (!success)
            {
                return Error(message);
            }

            return Ok(new AuthResponse
            {
                success = true,
                token = null,
                message = message
            });
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestBody request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Errors(errors);
            }

            var language = GetLanguage();
            var (success, message) = await _authService.ResendOtp(request, language);

            if (!success)
            {
                return Error(message);
            }

            return Ok(new AuthResponse
            {
                success = true,
                token = null,
                message = message
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestBody request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Errors(errors);
            }

            var language = GetLanguage();
            var (success, message) = await _authService.ForgotPassword(request, language);

            if (!success)
            {
                return Error(message, 404);
            }

            return Ok(new AuthResponse
            {
                success = true,
                token = null,
                message = message
            });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestBody request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Errors(errors);
            }

            var language = GetLanguage();

            // First verify OTP if needed (this endpoint is used after forgot password flow)
            var (success, message) = await _authService.ChangePassword(request, language);

            if (!success)
            {
                return Error(message);
            }

            return Ok(new AuthResponse
            {
                success = true,
                token = null,
                message = message
            });
        }
    }
}
