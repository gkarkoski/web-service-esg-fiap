using EsgTrainingManagement.Api.DTOs;
using FluentValidation;

namespace EsgTrainingManagement.Api.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(200);
    }
}
