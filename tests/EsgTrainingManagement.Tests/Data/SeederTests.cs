using EsgTrainingManagement.Api.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EsgTrainingManagement.Tests.Data;

public class SeederTests
{
    [Fact]
    public async Task SeedCompletesPartialCatalogWithoutOverwritingOperationalData()
    {
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.JobRoles.Add(new EsgTrainingManagement.Api.Models.JobRole { Id = 1, Name = "Old role" });
        await db.SaveChangesAsync();
        await DbSeeder.SeedAsync(db, TimeProvider.System);
        var employee = await db.Employees.FindAsync(DbSeeder.EmployeeId(1));
        employee!.Name = "Updated Employee";
        await db.SaveChangesAsync();
        await DbSeeder.SeedAsync(db, TimeProvider.System);
        Assert.Equal(3, await db.JobRoles.CountAsync());
        Assert.Equal(4, await db.Trainings.CountAsync());
        Assert.Equal(5, await db.Employees.CountAsync());
        Assert.Equal(12, await db.TrainingRequirements.CountAsync());
        Assert.Equal(12, await db.TrainingRecords.CountAsync());
        Assert.Equal("Updated Employee", employee.Name);
    }
}
