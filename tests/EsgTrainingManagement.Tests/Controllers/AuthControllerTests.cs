using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EsgTrainingManagement.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task LoginReturnsHttp200()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "admin@esg.com", password = "Admin@123" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task InvalidCredentialsReturnHttp401()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "admin@esg.com", password = "wrong" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
