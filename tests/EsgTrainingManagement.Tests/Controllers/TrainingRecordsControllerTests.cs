using System.Net;
using System.Net.Http.Json;
using EsgTrainingManagement.Api.Data;
using EsgTrainingManagement.Api.DTOs;
using Xunit;

namespace EsgTrainingManagement.Tests.Controllers;

public class TrainingRecordsControllerTests
{
    [Fact]
    public async Task ListReturnsHttp200()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = await factory.CreateSeededClientAsync();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/training-records?employeeId={DbSeeder.EmployeeId(1)}")).StatusCode);
    }

    [Fact]
    public async Task CreateCalculatesExpiryAndRejectsFutureCompletion()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = await factory.CreateSeededClientAsync();
        var completed = DateTime.UtcNow.AddDays(-1);
        var response = await client.PostAsJsonAsync("/api/training-records", new { employeeId = DbSeeder.EmployeeId(1), trainingId = 1, completedAt = completed, score = 9.5m, certificateCode = "TEST-001" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var record = await response.Content.ReadFromJsonAsync<TrainingRecordResponse>();
        Assert.Equal(completed.AddMonths(12), record!.ExpiresAt);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/training-records", new { employeeId = DbSeeder.EmployeeId(1), trainingId = 1, completedAt = DateTime.UtcNow.AddDays(1), score = 9, certificateCode = "FUTURE" })).StatusCode);
    }
}
