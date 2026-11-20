using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace EsgTrainingManagement.Api.Exceptions;

public class NotFoundException(string message) : Exception(message);
public class BusinessRuleException(string message) : Exception(message);

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, detail) = exception switch
        {
            NotFoundException => (404, exception.Message),
            BusinessRuleException => (422, exception.Message),
            DbUpdateException { InnerException: OracleException { Number: 1 } } => (422, "A resource with these unique values already exists."),
            _ => (500, "An unexpected error occurred.")
        };
        if (status == 500) logger.LogError(exception, "Unhandled request failure");
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = status == 500 ? "Internal server error" : status == 404 ? "Resource not found" : "Business rule violation",
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{status}"
        }, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);
        return true;
    }
}
