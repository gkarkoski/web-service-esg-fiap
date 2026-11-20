using EsgTrainingManagement.Api.Data;
using EsgTrainingManagement.Api.Enums;
using EsgTrainingManagement.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EsgTrainingManagement.Tests.Services;

public class FixedTimeProvider(DateTime now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(now);
}

public class ComplianceServiceTests
{
    private static readonly DateTime Now = new(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(-1, ComplianceStatus.Expired)]
    [InlineData(0, ComplianceStatus.Expired)]
    [InlineData(1, ComplianceStatus.ExpiringSoon)]
    [InlineData(30, ComplianceStatus.ExpiringSoon)]
    [InlineData(31, ComplianceStatus.Compliant)]
    public void ExpiryBoundaries(int days, ComplianceStatus expected) => Assert.Equal(expected, ComplianceService.Classify(Now, Now.AddDays(-100), 30, true, Now.AddDays(days)));

    [Fact]
    public void LifetimeAndHireDeadlineBoundaries()
    {
        Assert.Equal(ComplianceStatus.Compliant, ComplianceService.Classify(Now, Now, 0, true, null));
        Assert.Equal(ComplianceStatus.WithinDeadline, ComplianceService.Classify(Now, Now.AddDays(-29), 30, false, null));
        Assert.Equal(ComplianceStatus.Pending, ComplianceService.Classify(Now, Now.AddDays(-30), 30, false, null));
        Assert.Equal(ComplianceStatus.Pending, ComplianceService.Classify(Now, Now, 0, false, null));
    }

    [Fact]
    public void AdherenceExcludesObligationsWithinDeadline()
    {
        var summary = ComplianceService.Summarize([ComplianceStatus.Compliant, ComplianceStatus.ExpiringSoon, ComplianceStatus.Expired, ComplianceStatus.Pending, ComplianceStatus.WithinDeadline]);
        Assert.Equal(4, summary.DueObligations);
        Assert.Equal(50m, summary.AdherencePercentage);
        Assert.Equal(100m, ComplianceService.Summarize([ComplianceStatus.WithinDeadline]).AdherencePercentage);
        Assert.Equal(100m, ComplianceService.Summarize([]).AdherencePercentage);
    }

    [Fact]
    public async Task QueryUsesMostRecentCompletionAndUniqueTieBreaker()
    {
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        await DbSeeder.SeedAsync(db, new FixedTimeProvider(Now));
        var id = DbSeeder.EmployeeId(1);
        db.TrainingRecords.Add(new() { EmployeeId = id, TrainingId = 1, CompletedAt = Now.AddDays(-1), ExpiresAt = Now.AddDays(-1), CertificateCode = "NEWER-EXPIRED" });
        await db.SaveChangesAsync();
        var service = new ComplianceService(db, new FixedTimeProvider(Now), new EmployeeService(db));
        Assert.Equal(ComplianceStatus.Expired, (await service.Query(id).SingleAsync(x => x.TrainingId == 1)).Status);
        db.TrainingRecords.Add(new() { EmployeeId = id, TrainingId = 1, CompletedAt = Now.AddDays(-1), ExpiresAt = null, CertificateCode = "TIE-LIFETIME" });
        await db.SaveChangesAsync();
        Assert.Equal(ComplianceStatus.Compliant, (await service.Query(id).SingleAsync(x => x.TrainingId == 1)).Status);
    }

    [Fact]
    public void ComplianceFiltersAndPaginationTranslateToOracleSql()
    {
        using var db = new AppDbContextFactory().CreateDbContext([]);
        var service = new ComplianceService(db, new FixedTimeProvider(Now), new EmployeeService(db));
        var sql = service.Query(activeOnly: true).Where(x => x.Status == ComplianceStatus.Expired).OrderBy(x => x.EmployeeId).ThenBy(x => x.TrainingId).Skip(0).Take(10).ToQueryString();
        Assert.Contains("SELECT", sql);
        Assert.Contains("TrainingRecords", sql);
        var aggregates = service.Query(activeOnly: true).GroupBy(x => x.EsgPillar).Select(g => new
        {
            Pillar = g.Key,
            Compliant = g.Count(x => x.Status == ComplianceStatus.Compliant || x.Status == ComplianceStatus.ExpiringSoon),
            Due = g.Count(x => x.Status != ComplianceStatus.WithinDeadline)
        }).ToQueryString();
        Assert.Contains("GROUP BY", aggregates);
    }
}
