using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using EsgTrainingManagement.Api.Data;
using EsgTrainingManagement.Api.Exceptions;
using EsgTrainingManagement.Api.Security;
using EsgTrainingManagement.Api.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<AppDbContext>((sp, o) =>
    o.UseOracle(
        sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection") ??
        "Data Source=not-configured;",
        oracle => oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)));
builder.Services.AddSingleton(sp =>
{
    var jwt = sp.GetRequiredService<IConfiguration>().GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
    if (string.IsNullOrWhiteSpace(jwt.SigningKey))
        jwt.SigningKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    if (Encoding.UTF8.GetByteCount(jwt.SigningKey) < 32)
        throw new InvalidOperationException("Jwt:SigningKey must have at least 32 bytes when configured.");
    return jwt.ExpirationMinutes <= 0
        ? throw new InvalidOperationException("Jwt:ExpirationMinutes must be positive.")
        : jwt;
});
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<TrainingService>();
builder.Services.AddScoped<RequirementService>();
builder.Services.AddScoped<TrainingRecordService>();
builder.Services.AddScoped<ComplianceService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme).Configure<JwtSettings>((o, jwt) =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = jwt.Issuer, ValidateAudience = true, ValidAudience = jwt.Audience,
        ValidateLifetime = true, ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)), ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "ESG Training Management API", Version = "v1" });
    o.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" });
    o.OperationFilter<BearerSecurityOperationFilter>();
});
var app = builder.Build();
app.Services.GetRequiredService<JwtSettings>();
app.UseExceptionHandler();
app.UseStatusCodePages();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v1/swagger.json", "ESG Training Management API v1"));
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
if (!app.Environment.IsEnvironment("Testing"))
{
    var connection = app.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connection))
        app.Logger.LogWarning("Oracle connection is not configured. Database migration and seed were not executed.");
    else
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<TimeProvider>());
    }
}

app.Run();

public partial class Program;