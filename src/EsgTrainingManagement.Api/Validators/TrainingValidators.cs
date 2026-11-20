using EsgTrainingManagement.Api.DTOs;
using FluentValidation;

namespace EsgTrainingManagement.Api.Validators;

public class CreateTrainingValidator : AbstractValidator<CreateTrainingRequest>
{
    public CreateTrainingValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.EsgPillar).IsInEnum();
        RuleFor(x => x.DurationHours).GreaterThan(0);
        RuleFor(x => x.ValidityMonths).InclusiveBetween(0, 1200);
    }
}

public class TrainingQueryValidator : PaginationValidator<TrainingQuery>
{
    public TrainingQueryValidator()
    {
        RuleFor(x => x.EsgPillar).IsInEnum();
        RuleFor(x => x.Title).MaximumLength(150);
    }
}
