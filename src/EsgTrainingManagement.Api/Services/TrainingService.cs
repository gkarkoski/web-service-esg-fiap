using System.Linq.Expressions;
using EsgTrainingManagement.Api.Data;
using EsgTrainingManagement.Api.DTOs;
using EsgTrainingManagement.Api.Exceptions;
using EsgTrainingManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EsgTrainingManagement.Api.Services;

public class TrainingService(AppDbContext db)
{
    private static readonly Expression<Func<Training, TrainingResponse>> Projection = x => new(x.Id, x.Title, x.Description, x.EsgPillar, x.DurationHours, x.ValidityMonths);

    public Task<PagedResponse<TrainingResponse>> ListAsync(TrainingQuery query, CancellationToken ct) => db.Trainings.AsNoTracking()
        .Where(x => (!query.EsgPillar.HasValue || x.EsgPillar == query.EsgPillar) && (query.Title == null || x.Title.ToUpper().Contains(query.Title.ToUpper())))
        .OrderBy(x => x.Title).ThenBy(x => x.Id).Select(Projection).ToPageAsync(query, ct);

    public async Task<TrainingResponse> GetAsync(int id, CancellationToken ct) => await db.Trainings.AsNoTracking().Where(x => x.Id == id).Select(Projection).SingleOrDefaultAsync(ct) ?? throw new NotFoundException("Training not found.");

    public async Task<TrainingResponse> CreateAsync(CreateTrainingRequest request, CancellationToken ct)
    {
        var training = new Training { Title = request.Title.Trim(), Description = request.Description.Trim(), EsgPillar = request.EsgPillar, DurationHours = request.DurationHours, ValidityMonths = request.ValidityMonths };
        db.Trainings.Add(training);
        await db.SaveChangesAsync(ct);
        return await GetAsync(training.Id, ct);
    }
}
