using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace RepositoryLayer.Middleware
{
    public class UnauthorisedMiddleware
    {
        private readonly RequestDelegate _next;
        public UnauthorisedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Authentication failed. Please provide a valid JWT token."
                }));
            }
        }
    }
}
