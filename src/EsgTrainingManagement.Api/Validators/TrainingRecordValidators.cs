using EsgTrainingManagement.Api.DTOs;
using FluentValidation;

namespace EsgTrainingManagement.Api.Validators;

public class CreateTrainingRecordValidator : AbstractValidator<CreateTrainingRecordRequest>
{
    public CreateTrainingRecordValidator(TimeProvider clock)
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.TrainingId).GreaterThan(0);
        RuleFor(x => x.CompletedAt).Must(x => x.Kind == DateTimeKind.Utc && x > DateTime.MinValue && x <= clock.GetUtcNow().UtcDateTime).WithMessage("Completion must be a nonfuture UTC timestamp.");
        RuleFor(x => x.Score).InclusiveBetween(0, 10).PrecisionScale(5, 2, true);
        RuleFor(x => x.CertificateCode).NotEmpty().MaximumLength(60);
    }
}

public class TrainingRecordQueryValidator : PaginationValidator<TrainingRecordQuery>
{
    public TrainingRecordQueryValidator()
    {
        RuleFor(x => x.EmployeeId).Must(x => x is null || x != Guid.Empty);
        RuleFor(x => x.TrainingId).GreaterThan(0);
    }
}
