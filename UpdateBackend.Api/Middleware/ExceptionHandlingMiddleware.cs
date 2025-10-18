using System.Net;
using System.Text.Json;
using UpdateBackend.Api.DTOs;

namespace UpdateBackend.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>();

            switch (exception)
            {
                case ArgumentException argEx:
                    response = ApiResponse<object>.ErrorResponse(argEx.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case FileNotFoundException:
                    response = ApiResponse<object>.ErrorResponse("File not found");
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case UnauthorizedAccessException:
                    response = ApiResponse<object>.ErrorResponse("Access denied");
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                default:
                    response = ApiResponse<object>.ErrorResponse("An error occurred while processing your request");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
