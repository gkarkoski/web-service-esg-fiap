using System.Net;
using System.Net.Http.Json;
using EsgTrainingManagement.Api.DTOs;
using Xunit;

namespace EsgTrainingManagement.Tests.Controllers;

public class EmployeesControllerTests
{
    [Fact]
    public async Task ListReturnsHttp200()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = await factory.CreateSeededClientAsync();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/employees")).StatusCode);
    }

    [Fact]
    public async Task CreateGeneratesGuidAndLocationAndRejectsDuplicates()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = await factory.CreateSeededClientAsync();
        var request = new { name = "New Employee", employeeNumber = "NEW-001", email = "new@example.com", hireDate = DateTime.UtcNow.AddDays(-1), isActive = true, jobRole = "Operator" };
        var response = await client.PostAsJsonAsync("/api/employees", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var employee = await response.Content.ReadFromJsonAsync<EmployeeResponse>();
        Assert.NotEqual(Guid.Empty, employee!.Id);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(response.Headers.Location)).StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await client.PostAsJsonAsync("/api/employees", request)).StatusCode);
    }

    [Theory]
    [InlineData("/api/employees?pageSize=51", HttpStatusCode.BadRequest)]
    [InlineData("/api/employees?pageNumber=0", HttpStatusCode.BadRequest)]
    [InlineData("/api/employees/99999999-9999-9999-9999-999999999999", HttpStatusCode.NotFound)]
    public async Task InvalidRequestsReturnExpectedStatus(string path, HttpStatusCode status)
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = await factory.CreateSeededClientAsync();
        Assert.Equal(status, (await client.GetAsync(path)).StatusCode);
    }

    [Fact]
    public async Task AnonymousCannotReadAndAuditorCannotWrite()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var anonymous = await factory.CreateSeededClientAsync(null);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/employees")).StatusCode);
        using var auditor = await factory.CreateSeededClientAsync("Auditor");
        Assert.Equal(HttpStatusCode.Forbidden, (await auditor.PostAsJsonAsync("/api/employees", new { })).StatusCode);
    }
}
