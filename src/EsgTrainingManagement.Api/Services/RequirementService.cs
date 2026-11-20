using EsgTrainingManagement.Api.Data;
using EsgTrainingManagement.Api.DTOs;
using EsgTrainingManagement.Api.Exceptions;
using EsgTrainingManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EsgTrainingManagement.Api.Services;

public class RequirementService(AppDbContext db)
{
    public Task<PagedResponse<JobRoleResponse>> ListAsync(JobRoleQuery query, CancellationToken ct) => db.JobRoles.AsNoTracking().OrderBy(x => x.Id)
        .Select(x => new JobRoleResponse(x.Id, x.Name, x.Department, x.Description,
            x.TrainingRequirements.Where(r => r.IsMandatory).OrderBy(r => r.TrainingId)
                .Select(r => new RequirementResponse(r.TrainingId, r.Training.Title, r.DaysAfterHire)).ToList())).ToPageAsync(query, ct);

    public async Task CreateAsync(int jobRoleId, int trainingId, CreateRequirementRequest request, CancellationToken ct)
    {
        if (!await db.JobRoles.AnyAsync(x => x.Id == jobRoleId, ct)) throw new NotFoundException("Job role not found.");
        if (!await db.Trainings.AnyAsync(x => x.Id == trainingId, ct)) throw new NotFoundException("Training not found.");
        if (await db.TrainingRequirements.AnyAsync(x => x.JobRoleId == jobRoleId && x.TrainingId == trainingId, ct)) throw new BusinessRuleException("Training requirement already exists.");
        db.TrainingRequirements.Add(new TrainingRequirement { JobRoleId = jobRoleId, TrainingId = trainingId, DaysAfterHire = request.DaysAfterHire });
        await db.SaveChangesAsync(ct);
    }
}
