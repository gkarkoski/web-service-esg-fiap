# ESG Training Management

ASP.NET Core 8 REST API for corporate training requirements, certification expiry, and ESG compliance.

## Architecture

The project follows MVC adapted to a Web API, as agreed with the course professor. Models represent the domain, controllers handle HTTP requests, DTOs define API contracts, services implement business rules, and EF Core handles persistence. The API has no frontend or Razor views; DTOs are not views.

## Structure

```text
web-service-esg-fiap.sln
src/EsgTrainingManagement.Api/
tests/EsgTrainingManagement.Tests/
```

## Development

Use the .NET 8 SDK, or a newer SDK supporting the `net8.0` target. Running the API requires the ASP.NET Core 8 runtime, which is included in the .NET 8 SDK.

```sh
dotnet restore web-service-esg-fiap.sln
dotnet build web-service-esg-fiap.sln --no-restore
dotnet run --project src/EsgTrainingManagement.Api -- --urls http://localhost:8080
```

## Implementation status

The solution contains a controller-based Web API host and an xUnit test project targeting .NET 8 with C# 12. Dependencies for Oracle EF Core, validation, JWT, OpenAPI, Swagger, HTTP integration tests, and EF Core InMemory are configured.

Business endpoints, database mappings, migrations, authentication, and tests will be implemented in subsequent stages. The current host does not expose business routes or initialize a database. No tests have been implemented yet.

Keep Oracle credentials and signing secrets outside tracked files. Use environment variables or .NET user-secrets; examples must contain placeholders only.
