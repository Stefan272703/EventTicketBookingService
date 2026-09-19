using EventTicketBookingService.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace EventTicketBookingService.Middlewares
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleException(httpContext, ex);
            }
        }

        private async Task HandleException(HttpContext httpContext, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.Method={Method}, Path={Path}, RequestId={RequestId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.Request.Headers["x-request-id"]);

            if (httpContext.Response.HasStarted)
            {
                return;
            }

            var statusCode = MapStatusCode(ex);
            var title = MapTitle(ex);
            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var error = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Title = title,
                Status = statusCode,
                Detail = ex.Message,
                Type = "Ссылка на url документации",
                Instance = httpContext.Request.Path
            };

            await httpContext.Response.WriteAsJsonAsync(error);

        }

        private int MapStatusCode(Exception ex)
        {
            return ex switch
            {
                ValidationException ve => StatusCodes.Status400BadRequest,
                ResourceNotFoundException re => StatusCodes.Status404NotFound,
                NoAvailableSeatsException nase => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError

            };
        }

        private string MapTitle(Exception ex)
        {
            return ex switch
            {
                ValidationException ve => "Bad Request",
                ResourceNotFoundException re => "Not Found",
                NoAvailableSeatsException nase => "Conflict",
                _ => "Internal Server Error"

            };
        }
    }
}
