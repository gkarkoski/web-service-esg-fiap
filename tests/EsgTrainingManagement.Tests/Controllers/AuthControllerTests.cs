using System.Net;
using System.Net.Http.Json;
using EsgTrainingManagement.Api.Security;
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

    [Fact]
    public async Task ApiStartsWithoutConfiguredSigningKeyAndIssuesAnInstanceSpecificToken()
    {
        await using var factory = new CustomWebApplicationFactory(signingKey: "");
        using var client = factory.CreateClient();
        const string protectedPath = "/api/trainings";

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(protectedPath)).StatusCode);
        using var login = await client.PostAsJsonAsync("/api/auth/login", new { email = "admin@esg.com", password = "Admin@123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = await login.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(token);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token.Token);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(protectedPath)).StatusCode);

        await using var newInstance = new CustomWebApplicationFactory(signingKey: "");
        using var newClient = newInstance.CreateClient();
        newClient.DefaultRequestHeaders.Authorization = client.DefaultRequestHeaders.Authorization;
        Assert.Equal(HttpStatusCode.Unauthorized, (await newClient.GetAsync(protectedPath)).StatusCode);
    }
}
