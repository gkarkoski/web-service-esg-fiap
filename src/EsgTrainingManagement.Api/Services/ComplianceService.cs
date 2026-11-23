using EsgTrainingManagement.Api.Data;
using EsgTrainingManagement.Api.DTOs;
using EsgTrainingManagement.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace EsgTrainingManagement.Api.Services;

public class ComplianceService(AppDbContext db, TimeProvider clock, EmployeeService employees)
{
    public static ComplianceStatus Classify(DateTime now, DateTime hireDate, int daysAfterHire, bool hasRecord,
        DateTime? expiresAt)
    {
        if (!hasRecord)
            return now < hireDate.AddDays(daysAfterHire) ? ComplianceStatus.WithinDeadline : ComplianceStatus.Pending;
        if (expiresAt is null) return ComplianceStatus.Compliant;
        if (expiresAt <= now) return ComplianceStatus.Expired;
        return expiresAt <= now.AddDays(30) ? ComplianceStatus.ExpiringSoon : ComplianceStatus.Compliant;
    }

    public static AdherenceSummary Summarize(IEnumerable<ComplianceStatus> statuses)
    {
        var list = statuses.ToList();
        var compliant = list.Count(x => x is ComplianceStatus.Compliant or ComplianceStatus.ExpiringSoon);
        var due = list.Count(x => x != ComplianceStatus.WithinDeadline);
        return new(compliant, due, Percentage(compliant, due));
    }

    private static decimal Percentage(int compliant, int due) =>
        due == 0 ? 100m : Math.Round(100m * compliant / due, 2, MidpointRounding.AwayFromZero);

    public IQueryable<ComplianceRow> Query(Guid? employeeId = null, bool activeOnly = false)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var threshold = now.AddDays(30);
        return from employee in db.Employees.AsNoTracking()
            where (!employeeId.HasValue || employee.Id == employeeId) && (!activeOnly || employee.IsActive)
            from requirement in db.TrainingRequirements.AsNoTracking()
            where requirement.JobRoleId == employee.JobRoleId && requirement.IsMandatory
            let latest = db.TrainingRecords.AsNoTracking()
                .Where(r => r.EmployeeId == employee.Id && r.TrainingId == requirement.TrainingId)
                .OrderByDescending(r => r.CompletedAt).ThenByDescending(r => r.Id).FirstOrDefault()
            select new ComplianceRow
            {
                EmployeeId = employee.Id, EmployeeName = employee.Name, JobRoleId = employee.JobRoleId,
                Department = employee.JobRole.Department,
                TrainingId = requirement.TrainingId, TrainingTitle = requirement.Training.Title,
                EsgPillar = requirement.Training.EsgPillar,
                Deadline = employee.HireDate.AddDays(requirement.DaysAfterHire),
                CompletedAt = latest == null ? null : latest.CompletedAt,
                ExpiresAt = latest == null ? null : latest.ExpiresAt,
                Status = latest == null
                    ? (now < employee.HireDate.AddDays(requirement.DaysAfterHire)
                        ? ComplianceStatus.WithinDeadline
                        : ComplianceStatus.Pending)
                    : latest.ExpiresAt == null
                        ? ComplianceStatus.Compliant
                        : latest.ExpiresAt <= now
                            ? ComplianceStatus.Expired
                            : latest.ExpiresAt <= threshold
                                ? ComplianceStatus.ExpiringSoon
                                : ComplianceStatus.Compliant
            };
    }

    public async Task<EmployeeComplianceResponse> GetAsync(Guid employeeId, CancellationToken ct)
    {
        var employee = await employees.GetAsync(employeeId, ct);
        var rows = await Query(employeeId).OrderBy(x => x.TrainingId).ToListAsync(ct);
        return new(employee, rows, Summarize(rows.Select(x => x.Status)));
    }

    public Task<PagedResponse<ComplianceRow>> ListIssuesAsync(ComplianceQuery query, CancellationToken ct) =>
        Query(activeOnly: true)
            .Where(x => (x.Status == ComplianceStatus.Expired || x.Status == ComplianceStatus.Pending ||
                         x.Status == ComplianceStatus.ExpiringSoon)
                        && (!query.Status.HasValue || x.Status == query.Status) &&
                        (!query.Department.HasValue || x.Department == query.Department))
            .OrderBy(x => x.EmployeeName).ThenBy(x => x.EmployeeId).ThenBy(x => x.TrainingId).ToPageAsync(query, ct);

    public async Task<IReadOnlyList<EsgIndicatorResponse>> IndicatorsAsync(CancellationToken ct)
    {
        var groups = await Query(activeOnly: true).GroupBy(x => x.EsgPillar).Select(g => new
        {
            Pillar = g.Key,
            Compliant =
                g.Count(x => x.Status == ComplianceStatus.Compliant || x.Status == ComplianceStatus.ExpiringSoon),
            Due = g.Count(x => x.Status != ComplianceStatus.WithinDeadline)
        }).ToListAsync(ct);
        return Enum.GetValues<EsgPillar>().Select(p =>
        {
            var group = groups.SingleOrDefault(x => x.Pillar == p);
            return new EsgIndicatorResponse(p, group?.Compliant ?? 0, group?.Due ?? 0,
                Percentage(group?.Compliant ?? 0, group?.Due ?? 0));
        }).ToList();
    }
}