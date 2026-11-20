using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace EsgTrainingManagement.Tests.Controllers;

public class CollectionWorkflowTests
{
    [Fact]
    public async Task CollectionRequestsExecuteInOrderAgainstTheApi()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "web-service-esg-fiap.sln"))) root = root.Parent;
        Assert.NotNull(root);
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(root!.FullName, "collections", "EsgTrainingManagement.postman_collection.json")));
        var variables = document.RootElement.GetProperty("variable").EnumerateArray().ToDictionary(x => x.GetProperty("key").GetString()!, x => x.GetProperty("value").GetString()!);
        variables["base_url"] = "";
        await using var factory = new CustomWebApplicationFactory();
        using var client = await factory.CreateSeededClientAsync(null);
        foreach (var folder in document.RootElement.GetProperty("item").EnumerateArray())
        foreach (var item in folder.GetProperty("item").EnumerateArray())
        {
            string Substitute(string text)
            {
                foreach (var variable in variables) text = text.Replace("{{" + variable.Key + "}}", variable.Value);
                return text;
            }
            var request = item.GetProperty("request");
            var method = request.GetProperty("method").GetString()!;
            var url = Substitute(request.GetProperty("url").GetString()!);
            using var message = new HttpRequestMessage(new HttpMethod(method), url);
            if (request.TryGetProperty("body", out var body)) message.Content = new StringContent(Substitute(body.GetProperty("raw").GetString()!), Encoding.UTF8, "application/json");
            var response = await client.SendAsync(message);
            var expected = method == "GET" || url == "/api/auth/login" ? HttpStatusCode.OK : url.StartsWith("/api/job-roles/") ? HttpStatusCode.NoContent : HttpStatusCode.Created;
            Assert.True(expected == response.StatusCode, $"{item.GetProperty("name").GetString()}: expected {expected}, got {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
            if (method != "POST" || expected == HttpStatusCode.NoContent) continue;
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (url == "/api/auth/login") client.DefaultRequestHeaders.Authorization = new("Bearer", json.GetProperty("token").GetString());
            else if (url == "/api/employees") variables["employee_id"] = json.GetProperty("id").GetString()!;
            else if (url == "/api/trainings") variables["training_id"] = json.GetProperty("id").ToString();
            else if (url == "/api/training-records") variables["record_id"] = json.GetProperty("id").ToString();
        }
    }

    [Fact]
    public async Task DevelopmentSwaggerIncludesAllSixControllers()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var development = factory.WithWebHostBuilder(b => b.UseEnvironment("Development"));
        using var client = development.CreateClient();
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Matches(@"^3\.\d+\.\d+$", json.GetProperty("openapi").GetString()!);
        var ui = await client.GetStringAsync("/swagger/index.js");
        Assert.Contains("v1/swagger.json", ui);
        var paths = json.GetProperty("paths");
        foreach (var path in new[] { "/api/auth/login", "/api/employees", "/api/trainings", "/api/job-roles", "/api/training-records", "/api/compliance/issues" })
            Assert.True(paths.TryGetProperty(path, out _));
    }
}
