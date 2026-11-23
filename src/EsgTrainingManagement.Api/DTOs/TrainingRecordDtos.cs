namespace EsgTrainingManagement.Api.DTOs;

public record CreateTrainingRecordRequest(
    Guid EmployeeId,
    int TrainingId,
    DateTime CompletedAt,
    decimal? Score,
    string CertificateCode);

public record TrainingRecordResponse(
    int Id,
    Guid EmployeeId,
    string EmployeeName,
    int TrainingId,
    string TrainingTitle,
    DateTime CompletedAt,
    DateTime? ExpiresAt,
    decimal? Score,
    string CertificateCode);

public class TrainingRecordQuery : PaginationParams
{
    public Guid? EmployeeId { get; set; }
    public int? TrainingId { get; set; }
}