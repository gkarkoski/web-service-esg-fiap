using EsgTrainingManagement.Api.Enums;

namespace EsgTrainingManagement.Api.DTOs;

public record CreateRequirementRequest(int DaysAfterHire);
public record RequirementResponse(int TrainingId, string TrainingTitle, int DaysAfterHire);
public record JobRoleResponse(int Id, string Name, Department Department, string Description, List<RequirementResponse> MandatoryTrainings);
public class JobRoleQuery : PaginationParams;
