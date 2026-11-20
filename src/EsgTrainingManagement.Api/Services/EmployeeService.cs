using System.Linq.Expressions;
using EsgTrainingManagement.Api.Data;
using EsgTrainingManagement.Api.DTOs;
using EsgTrainingManagement.Api.Exceptions;
using EsgTrainingManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EsgTrainingManagement.Api.Services;

public class EmployeeService(AppDbContext db)
{
    private static readonly Expression<Func<Employee, EmployeeResponse>> Projection = x => new(x.Id, x.Name, x.EmployeeNumber, x.Email, x.HireDate, x.IsActive, x.JobRoleId, x.JobRole.Name);

    public Task<PagedResponse<EmployeeResponse>> ListAsync(EmployeeQuery query, CancellationToken ct) => db.Employees.AsNoTracking()
        .Where(x => (!query.JobRoleId.HasValue || x.JobRoleId == query.JobRoleId) && (!query.IsActive.HasValue || x.IsActive == query.IsActive))
        .OrderBy(x => x.Name).ThenBy(x => x.Id).Select(Projection).ToPageAsync(query, ct);

    public async Task<EmployeeResponse> GetAsync(Guid id, CancellationToken ct) =>
        await db.Employees.AsNoTracking().Where(x => x.Id == id).Select(Projection).SingleOrDefaultAsync(ct) ?? throw new NotFoundException("Employee not found.");

    public async Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken ct)
    {
        var number = request.EmployeeNumber.Trim().ToUpperInvariant();
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Employees.AnyAsync(x => x.EmployeeNumber == number || x.Email == email, ct))
            throw new BusinessRuleException("Employee number and email must be unique.");
        if (!await db.JobRoles.AnyAsync(x => x.Id == (int)request.JobRole, ct)) throw new BusinessRuleException("Job role is not available in the reference catalog.");
        var employee = new Employee { Id = Guid.NewGuid(), Name = request.Name.Trim(), EmployeeNumber = number, Email = email, HireDate = request.HireDate, IsActive = request.IsActive, JobRoleId = (int)request.JobRole };
        db.Employees.Add(employee);
        await db.SaveChangesAsync(ct);
        return await GetAsync(employee.Id, ct);
    }
}
