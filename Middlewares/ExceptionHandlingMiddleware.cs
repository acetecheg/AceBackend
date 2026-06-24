using AceBackend.DTOs;
using AceBackend.Helpers;
using System.Net;
using System.Text.Json;

namespace AceBackend.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly LocalizationHelper _localization;

        public ExceptionHandlingMiddleware(RequestDelegate next, LocalizationHelper localization)
        {
            _next = next;
            _localization = localization;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var language = context.Items["Language"]?.ToString() ?? "en";
            
            Console.WriteLine($"Unhandled exception: {exception}");

            var response = new ApiErrorModel
            {
                success = false,
                errors = new List<string> { _localization.Get("auth.common.serverError", language) }
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}
