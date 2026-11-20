using System.Net;
using System.Net.Http.Json;
using EsgTrainingManagement.Api.DTOs;
using Xunit;

namespace EsgTrainingManagement.Tests.Controllers;

public class TrainingRequirementsControllerTests
{
    [Fact]
    public async Task ListReturnsHttp200()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = await factory.CreateSeededClientAsync();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/job-roles")).StatusCode);
    }

    [Fact]
    public async Task LinkReturns204AndDuplicateReturns422()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = await factory.CreateSeededClientAsync();
        var created = await client.PostAsJsonAsync("/api/trainings", new { title = "New Requirement", description = "Description", esgPillar = "Social", durationHours = 4, validityMonths = 12 });
        var training = await created.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var path = $"/api/job-roles/1/trainings/{training.GetProperty("id").GetInt32()}";
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync(path, new { daysAfterHire = 30 })).StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await client.PostAsJsonAsync(path, new { daysAfterHire = 30 })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/job-roles/99/trainings/1", new { daysAfterHire = 30 })).StatusCode);
    }
}
