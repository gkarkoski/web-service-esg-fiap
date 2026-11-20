using EsgTrainingManagement.Api.Enums;

namespace EsgTrainingManagement.Api.DTOs;

public record CreateTrainingRequest(string Title, string Description, EsgPillar EsgPillar, int DurationHours, int ValidityMonths);
public record TrainingResponse(int Id, string Title, string Description, EsgPillar EsgPillar, int DurationHours, int ValidityMonths);
public class TrainingQuery : PaginationParams
{
    public EsgPillar? EsgPillar { get; set; }
    public string? Title { get; set; }
}
