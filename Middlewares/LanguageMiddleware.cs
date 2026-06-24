namespace AceBackend.Middlewares
{
    public class LanguageMiddleware
    {
        private readonly RequestDelegate _next;

        public LanguageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var language = context.Request.Headers["Accept-Language"].FirstOrDefault() ?? "en";
            
            // Store language in HttpContext.Items for access in controllers
            context.Items["Language"] = language.ToLower() == "ar" ? "ar" : "en";

            await _next(context);
        }
    }
}
