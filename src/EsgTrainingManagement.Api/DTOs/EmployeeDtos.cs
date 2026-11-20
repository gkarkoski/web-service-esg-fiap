using EsgTrainingManagement.Api.Enums;

namespace EsgTrainingManagement.Api.DTOs;

public record CreateEmployeeRequest(string Name, string EmployeeNumber, string Email, DateTime HireDate, bool IsActive, JobRoleType JobRole);
public record EmployeeResponse(Guid Id, string Name, string EmployeeNumber, string Email, DateTime HireDate, bool IsActive, int JobRoleId, string JobRoleName);
public class EmployeeQuery : PaginationParams
{
    public int? JobRoleId { get; set; }
    public bool? IsActive { get; set; }
}
