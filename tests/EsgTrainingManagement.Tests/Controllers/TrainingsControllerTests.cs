using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EsgTrainingManagement.Tests.Controllers;

public class TrainingsControllerTests
{
    [Fact]
    public async Task ListReturnsHttp200()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = await factory.CreateSeededClientAsync();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/trainings?esgPillar=Social&pageSize=2")).StatusCode);
    }

    [Fact]
    public async Task CreateReturns201AndUnknownEnumReturns400()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = await factory.CreateSeededClientAsync();
        var response = await client.PostAsJsonAsync("/api/trainings", new { title = "New Training", description = "Description", esgPillar = "Environmental", durationHours = 2, validityMonths = 0 });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(response.Headers.Location)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/trainings?esgPillar=999")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/trainings", new { title = "Invalid", esgPillar = "Unknown", durationHours = 0, validityMonths = -1 })).StatusCode);
    }
}
