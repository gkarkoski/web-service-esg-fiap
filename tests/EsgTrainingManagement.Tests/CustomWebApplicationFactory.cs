using System.Net.Http.Json;
using EsgTrainingManagement.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EsgTrainingManagement.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = Guid.NewGuid().ToString();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SigningKey"] = "test-only-signing-key-with-at-least-32-bytes"
        }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(databaseName));
        });
    }

    public async Task<HttpClient> CreateSeededClientAsync(string? role = "Admin")
    {
        var client = CreateClient();
        using var scope = Services.CreateScope();
        await DbSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>(), TimeProvider.System);
        if (role is not null)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", new { email = role == "Admin" ? "admin@esg.com" : "auditor@esg.com", password = role == "Admin" ? "Admin@123" : "Auditor@123" });
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadFromJsonAsync<EsgTrainingManagement.Api.Security.TokenResponse>();
            client.DefaultRequestHeaders.Authorization = new("Bearer", token!.Token);
        }
        return client;
    }
}
