using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace EsgTrainingManagement.Tests.Controllers;

public class SecurityTests
{
    [Theory]
    [InlineData("/api/employees")]
    [InlineData("/api/trainings")]
    [InlineData("/api/job-roles")]
    [InlineData("/api/training-records")]
    [InlineData("/api/compliance/issues")]
    [InlineData("/api/compliance/esg-indicators")]
    public async Task AnonymousAndInvalidTokensCannotRead(string path)
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(path)).StatusCode);
        client.DefaultRequestHeaders.Authorization = new("Bearer", "invalid.jwt.token");
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(path)).StatusCode);
    }

    [Theory]
    [InlineData("/api/employees")]
    [InlineData("/api/trainings")]
    [InlineData("/api/job-roles/1/trainings/1")]
    [InlineData("/api/training-records")]
    public async Task AuditorCannotWrite(string path)
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = await factory.CreateSeededClientAsync("Auditor");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync(path, new { })).StatusCode);
    }

    [Theory]
    [InlineData("/api/trainings?pageSize=0")]
    [InlineData("/api/job-roles?pageNumber=-1")]
    [InlineData("/api/training-records?pageSize=51")]
    [InlineData("/api/compliance/issues?pageSize=51")]
    [InlineData("/api/compliance/issues?status=WithinDeadline")]
    [InlineData("/api/compliance/issues?department=999")]
    public async Task InvalidQueriesReturn400(string path)
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = await factory.CreateSeededClientAsync();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync(path)).StatusCode);
    }

    [Fact]
    public async Task NotFoundUsesProblemDetailsWithoutInternalErrors()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = await factory.CreateSeededClientAsync();
        var response = await client.GetAsync("/api/compliance/employees/99999999-9999-9999-9999-999999999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(404, json.GetProperty("status").GetInt32());
        Assert.False(json.TryGetProperty("stackTrace", out _));
    }
}
