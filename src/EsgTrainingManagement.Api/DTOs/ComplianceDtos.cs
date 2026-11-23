using EsgTrainingManagement.Api.Enums;

namespace EsgTrainingManagement.Api.DTOs;

public class ComplianceRow
{
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = "";
    public int JobRoleId { get; init; }
    public Department Department { get; init; }
    public int TrainingId { get; init; }
    public string TrainingTitle { get; init; } = "";
    public EsgPillar EsgPillar { get; init; }
    public DateTime Deadline { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public ComplianceStatus Status { get; init; }
}

public record AdherenceSummary(int CompliantObligations, int DueObligations, decimal AdherencePercentage);

public record EmployeeComplianceResponse(
    EmployeeResponse Employee,
    IReadOnlyList<ComplianceRow> MandatoryTrainings,
    AdherenceSummary Summary);

public record EsgIndicatorResponse(
    EsgPillar EsgPillar,
    int CompliantObligations,
    int DueObligations,
    decimal AdherencePercentage);

public class ComplianceQuery : PaginationParams
{
    public ComplianceStatus? Status { get; set; }
    public Department? Department { get; set; }
}