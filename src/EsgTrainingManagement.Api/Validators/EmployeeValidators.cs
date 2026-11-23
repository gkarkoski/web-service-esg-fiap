using EsgTrainingManagement.Api.DTOs;
using EsgTrainingManagement.Api.Enums;
using FluentValidation;

namespace EsgTrainingManagement.Api.Validators;

public class PaginationValidator<T> : AbstractValidator<T> where T : PaginationParams
{
    public PaginationValidator()
    {
        RuleFor(x => x.PageNumber).InclusiveBetween(1, int.MaxValue / 50);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}

public class EmployeeQueryValidator : PaginationValidator<EmployeeQuery>
{
    public EmployeeQueryValidator() => RuleFor(x => x.JobRoleId)
        .Must(x => x is null || Enum.IsDefined((JobRoleType)x.Value)).WithMessage("Unknown job role.");
}

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeValidator(TimeProvider clock)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.JobRole).IsInEnum();
        RuleFor(x => x.HireDate)
            .Must(x => x.Kind == DateTimeKind.Utc && x <= clock.GetUtcNow().UtcDateTime && x > DateTime.MinValue)
            .WithMessage("Hire date must be a nonfuture UTC timestamp.");
    }
}