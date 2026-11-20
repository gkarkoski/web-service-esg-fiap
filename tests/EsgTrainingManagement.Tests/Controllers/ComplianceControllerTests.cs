using System.Net;
using Xunit;

namespace EsgTrainingManagement.Tests.Controllers;

public class ComplianceControllerTests
{
    [Theory]
    [InlineData("/api/compliance/employees/00000000-0000-0000-0000-000000000001")]
    [InlineData("/api/compliance/issues")]
    [InlineData("/api/compliance/esg-indicators")]
    public async Task GetReturnsHttp200(string path)
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = await factory.CreateSeededClientAsync();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(path)).StatusCode);
    }
}
