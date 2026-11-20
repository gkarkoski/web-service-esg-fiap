using EsgTrainingManagement.Api.Data;
using EsgTrainingManagement.Api.Enums;
using EsgTrainingManagement.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EsgTrainingManagement.Tests.Services;

public class ComplianceQueryTests
{
    [Fact]
    public async Task QueryMatchesClockBoundariesAndIgnoresOptionalRequirements()
    {
        var now = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var clock = new FixedTimeProvider(now);
        await DbSeeder.SeedAsync(db, clock);
        var service = new ComplianceService(db, clock, new EmployeeService(db));
        var pending = await service.GetAsync(DbSeeder.EmployeeId(4), default);
        Assert.All(pending.MandatoryTrainings, x => Assert.Equal(ComplianceStatus.Pending, x.Status));
        var within = await service.GetAsync(DbSeeder.EmployeeId(5), default);
        Assert.All(within.MandatoryTrainings, x => Assert.Equal(ComplianceStatus.WithinDeadline, x.Status));
        Assert.Equal(0, within.Summary.DueObligations);
        Assert.Equal(100m, within.Summary.AdherencePercentage);
        var record = await db.TrainingRecords.SingleAsync(x => x.EmployeeId == DbSeeder.EmployeeId(1) && x.TrainingId == 1);
        foreach (var (expiry, expected) in new[] { (now, ComplianceStatus.Expired), (now.AddTicks(1), ComplianceStatus.ExpiringSoon), (now.AddDays(30), ComplianceStatus.ExpiringSoon), (now.AddDays(30).AddTicks(1), ComplianceStatus.Compliant) })
        {
            record.ExpiresAt = expiry;
            await db.SaveChangesAsync();
            Assert.Equal(expected, (await service.Query(DbSeeder.EmployeeId(1)).SingleAsync(x => x.TrainingId == 1)).Status);
        }
        var requirement = await db.TrainingRequirements.FindAsync(1, 1);
        requirement!.IsMandatory = false;
        await db.SaveChangesAsync();
        Assert.DoesNotContain((await service.GetAsync(DbSeeder.EmployeeId(1), default)).MandatoryTrainings, x => x.TrainingId == 1);
    }

    [Fact]
    public async Task IssuesExcludeInactiveEmployeesAndIndicatorsUseDueObligations()
    {
        var clock = new FixedTimeProvider(new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc));
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        await DbSeeder.SeedAsync(db, clock);
        var service = new ComplianceService(db, clock, new EmployeeService(db));
        var issues = await service.ListIssuesAsync(new() { PageSize = 50 }, default);
        Assert.Equal(10, issues.TotalCount);
        Assert.DoesNotContain(issues.Items, x => x.Status == ComplianceStatus.WithinDeadline);
        var indicators = await service.IndicatorsAsync(default);
        Assert.Equal(50m, indicators.Single(x => x.EsgPillar == EsgPillar.Environmental).AdherencePercentage);
        Assert.Equal(62.5m, indicators.Single(x => x.EsgPillar == EsgPillar.Governance).AdherencePercentage);
        var employee = await db.Employees.FindAsync(DbSeeder.EmployeeId(4));
        employee!.IsActive = false;
        await db.SaveChangesAsync();
        Assert.Equal(6, (await service.ListIssuesAsync(new() { PageSize = 50 }, default)).TotalCount);
    }
}
