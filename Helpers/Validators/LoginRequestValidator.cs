using Authentication.DTOs.Auth;
using FluentValidation;

namespace Authentication.Helpers.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .MustBeValidEmail();

        RuleFor(x => x.Password)
            .NotEmpty()
                .WithMessage("Password is required.")
            .MaximumLength(100)
                .WithMessage("Password must not exceed 100 characters.");
    }
}