using EsgTrainingManagement.Api.DTOs;
using FluentValidation;

namespace EsgTrainingManagement.Api.Validators;

public class JobRoleQueryValidator : PaginationValidator<JobRoleQuery>;
public class CreateRequirementValidator : AbstractValidator<CreateRequirementRequest>
{
    public CreateRequirementValidator() => RuleFor(x => x.DaysAfterHire).InclusiveBetween(0, 36500);
}
