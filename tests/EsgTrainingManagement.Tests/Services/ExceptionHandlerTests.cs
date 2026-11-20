using EsgTrainingManagement.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EsgTrainingManagement.Tests.Services;

public class ExceptionHandlerTests
{
    [Fact]
    public async Task UnexpectedFailureDoesNotExposeExceptionDetails()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        await handler.TryHandleAsync(context, new InvalidOperationException("sensitive-database-details"), default);
        Assert.Equal(500, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.DoesNotContain("sensitive-database-details", body);
    }
}
