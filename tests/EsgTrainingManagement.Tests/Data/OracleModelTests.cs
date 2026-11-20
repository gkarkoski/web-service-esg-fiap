using EsgTrainingManagement.Api.Data;
using EsgTrainingManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace EsgTrainingManagement.Tests.Data;

public class OracleModelTests
{
    [Fact]
    public void EmployeeGuidAndForeignKeyUseMatchingRawMappings()
    {
        using var db = new AppDbContextFactory().CreateDbContext([]);
        var id = db.Model.FindEntityType(typeof(Employee))!.FindProperty(nameof(Employee.Id))!;
        var foreignKey = db.Model.FindEntityType(typeof(TrainingRecord))!.FindProperty(nameof(TrainingRecord.EmployeeId))!;
        Assert.Equal("RAW(16)", id.GetColumnType());
        Assert.Equal(id.GetColumnType(), foreignKey.GetColumnType());
        var value = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var converter = id.GetTypeMapping().Converter;
        if (converter is not null)
            Assert.Equal(value, converter.ConvertFromProvider(converter.ConvertToProvider(value)));
        Assert.Equal(id.GetTypeMapping().Converter?.ProviderClrType, foreignKey.GetTypeMapping().Converter?.ProviderClrType);
    }

    [Fact]
    public void OracleMigrationScriptIncludesRequiredTablesAndPrecision()
    {
        using var db = new AppDbContextFactory().CreateDbContext([]);
        var script = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>().GenerateScript();
        Assert.Contains("RAW(16)", script);
        Assert.Contains("NUMBER(5,2)", script);
        Assert.Contains("TrainingRequirements", script);
    }
}
