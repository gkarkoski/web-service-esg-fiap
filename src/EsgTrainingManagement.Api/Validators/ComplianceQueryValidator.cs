using EsgTrainingManagement.Api.DTOs;
using EsgTrainingManagement.Api.Enums;
using FluentValidation;

namespace EsgTrainingManagement.Api.Validators;

public class ComplianceQueryValidator : PaginationValidator<ComplianceQuery>
{
    public ComplianceQueryValidator()
    {
        RuleFor(x => x.Status)
            .Must(x => x is null or ComplianceStatus.Expired or ComplianceStatus.Pending
                or ComplianceStatus.ExpiringSoon).WithMessage("Status must be Expired, Pending or ExpiringSoon.");
        RuleFor(x => x.Department).IsInEnum();
    }
}