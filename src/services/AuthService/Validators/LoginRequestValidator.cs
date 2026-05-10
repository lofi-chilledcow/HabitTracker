using AuthService.DTOs;
using FluentValidation;

namespace AuthService.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty()
            .WithMessage("Email, username, or phone number is required.");

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
