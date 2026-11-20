using EsgTrainingManagement.Api.Enums;
using EsgTrainingManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EsgTrainingManagement.Api.Data;

public static class DbSeeder
{
    public static Guid EmployeeId(int number) => Guid.Parse($"00000000-0000-0000-0000-{number:D12}");

    public static async Task SeedAsync(AppDbContext db, TimeProvider clock, CancellationToken ct = default)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var roles = new[]
        {
            new JobRole { Id = 1, Name = "Operator", Department = Department.Operations, Description = "Industrial operations and workplace safety" },
            new JobRole { Id = 2, Name = "People Analyst", Department = Department.HumanResources, Description = "People development and privacy" },
            new JobRole { Id = 3, Name = "Finance Analyst", Department = Department.Finance, Description = "Financial governance and integrity" }
        };
        foreach (var role in roles)
        {
            var existing = await db.JobRoles.FindAsync([role.Id], ct);
            if (existing is null) db.JobRoles.Add(role);
            else { existing.Name = role.Name; existing.Description = role.Description; existing.Department = role.Department; }
        }
        await db.SaveChangesAsync(ct);
        var examples = new[]
        {
            new Training { Title = "Workplace Safety NR-01", Description = "Occupational risk prevention", EsgPillar = EsgPillar.Social, DurationHours = 8, ValidityMonths = 12 },
            new Training { Title = "Ethics and Anti-corruption", Description = "Integrity and ethical conduct", EsgPillar = EsgPillar.Governance, DurationHours = 4, ValidityMonths = 12 },
            new Training { Title = "LGPD and Data Privacy", Description = "Personal data protection", EsgPillar = EsgPillar.Governance, DurationHours = 4, ValidityMonths = 0 },
            new Training { Title = "Environmental Education", Description = "Waste management and sustainability", EsgPillar = EsgPillar.Environmental, DurationHours = 2, ValidityMonths = 12 }
        };
        foreach (var training in examples)
        {
            var existing = await db.Trainings.AsNoTracking().Where(x => x.Title == training.Title).Select(x => new { x.Id }).FirstOrDefaultAsync(ct);
            if (existing is null) { db.Trainings.Add(training); await db.SaveChangesAsync(ct); }
            else training.Id = existing.Id;
        }
        for (var role = 1; role <= 3; role++)
        foreach (var training in examples)
        {
            if (!await db.TrainingRequirements.AnyAsync(x => x.JobRoleId == role && x.TrainingId == training.Id, ct))
                db.TrainingRequirements.Add(new TrainingRequirement { JobRoleId = role, TrainingId = training.Id, DaysAfterHire = 30 });
        }
        for (var i = 1; i <= 5; i++)
        {
            var id = EmployeeId(i);
            if (!await db.Employees.AnyAsync(x => x.Id == id, ct))
                db.Employees.Add(new Employee { Id = id, Name = $"Demo Employee {i}", Email = $"employee{i}@example.com", EmployeeNumber = $"DEMO-{i:D3}", HireDate = now.AddDays(i == 5 ? -5 : -400), IsActive = true, JobRoleId = (i - 1) % 3 + 1 });
        }
        await db.SaveChangesAsync(ct);
        for (var i = 1; i <= 3; i++)
        foreach (var training in examples)
        {
            var code = $"DEMO-{i}-{training.Id}";
            if (await db.TrainingRecords.AnyAsync(x => x.CertificateCode == code, ct)) continue;
            var completed = i == 2 ? now.AddMonths(-13) : i == 3 ? now.AddMonths(-12).AddDays(15) : now.AddMonths(-1);
            db.TrainingRecords.Add(new TrainingRecord { EmployeeId = EmployeeId(i), TrainingId = training.Id, CompletedAt = completed, ExpiresAt = training.ValidityMonths == 0 ? null : completed.AddMonths(training.ValidityMonths), Score = 9, CertificateCode = code });
        }
        await db.SaveChangesAsync(ct);
    }
}
