using System.Text.Json;
using ManoMana.Application.Contracts;
using ManoMana.Application.Exceptions;

namespace ManoMana.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (status, code, message) = exception switch
            {
                ValidationException value => (400, value.Code, value.Message),
                UnauthorizedException value => (401, value.Code, value.Message),
                ForbiddenException value => (403, value.Code, value.Message),
                NotFoundException value => (404, value.Code, value.Message),
                ConflictException value => (409, value.Code, value.Message),
                _ => (500, "UNEXPECTED_ERROR", "An unexpected error occurred.")
            };
            if (status == 500) logger.LogError(exception, "Unhandled request exception");
            else logger.LogWarning("Request failed with {Code}: {Message}", code, message);
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse(code, message), cancellationToken: context.RequestAborted);
        }
    }
}
