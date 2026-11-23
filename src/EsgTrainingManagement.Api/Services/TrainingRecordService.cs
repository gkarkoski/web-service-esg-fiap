using System.Linq.Expressions;
using EsgTrainingManagement.Api.Data;
using EsgTrainingManagement.Api.DTOs;
using EsgTrainingManagement.Api.Exceptions;
using EsgTrainingManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EsgTrainingManagement.Api.Services;

public class TrainingRecordService(AppDbContext db)
{
    private static readonly Expression<Func<TrainingRecord, TrainingRecordResponse>> Projection = x =>
        new(x.Id, x.EmployeeId, x.Employee.Name, x.TrainingId, x.Training.Title, x.CompletedAt, x.ExpiresAt, x.Score,
            x.CertificateCode);

    public static DateTime? CalculateExpiry(DateTime completedAt, int validityMonths)
    {
        if (validityMonths < 0) throw new BusinessRuleException("Validity months cannot be negative.");
        try
        {
            return validityMonths == 0 ? null : completedAt.AddMonths(validityMonths);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new BusinessRuleException("Calculated expiry is outside the supported date range.");
        }
    }

    public Task<PagedResponse<TrainingRecordResponse>> ListAsync(TrainingRecordQuery query, CancellationToken ct) => db
        .TrainingRecords.AsNoTracking()
        .Where(x => (!query.EmployeeId.HasValue || x.EmployeeId == query.EmployeeId) &&
                    (!query.TrainingId.HasValue || x.TrainingId == query.TrainingId))
        .OrderByDescending(x => x.CompletedAt).ThenByDescending(x => x.Id).Select(Projection).ToPageAsync(query, ct);

    public async Task<TrainingRecordResponse> GetAsync(int id, CancellationToken ct) =>
        await db.TrainingRecords.AsNoTracking().Where(x => x.Id == id).Select(Projection).SingleOrDefaultAsync(ct) ??
        throw new NotFoundException("Training record not found.");

    public async Task<TrainingRecordResponse> CreateAsync(CreateTrainingRecordRequest request, CancellationToken ct)
    {
        if (!await db.Employees.AnyAsync(x => x.Id == request.EmployeeId, ct))
            throw new NotFoundException("Employee not found.");
        var training =
            await db.Trainings.AsNoTracking().Where(x => x.Id == request.TrainingId)
                .Select(x => new { x.ValidityMonths }).SingleOrDefaultAsync(ct) ??
            throw new NotFoundException("Training not found.");
        var record = new TrainingRecord
        {
            EmployeeId = request.EmployeeId, TrainingId = request.TrainingId, CompletedAt = request.CompletedAt,
            ExpiresAt = CalculateExpiry(request.CompletedAt, training.ValidityMonths), Score = request.Score,
            CertificateCode = request.CertificateCode.Trim()
        };
        db.TrainingRecords.Add(record);
        await db.SaveChangesAsync(ct);
        return await GetAsync(record.Id, ct);
    }
}