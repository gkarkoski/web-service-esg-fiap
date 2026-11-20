using EsgTrainingManagement.Api.Services;
using Xunit;

namespace EsgTrainingManagement.Tests.Services;

public class TrainingRecordServiceTests
{
    [Fact]
    public void LifetimeTrainingHasNoExpiry() => Assert.Null(TrainingRecordService.CalculateExpiry(new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc), 0));

    [Fact]
    public void ExpiryUsesCalendarMonths() => Assert.Equal(new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc), TrainingRecordService.CalculateExpiry(new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc), 1));
}
