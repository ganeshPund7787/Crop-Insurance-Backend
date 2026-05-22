using Authentication.DTOs.Auth;
using FluentValidation;

namespace Authentication.Helpers.Validators;

public class RefreshTokenRequestValidator
    : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
                .WithMessage("Refresh token is required.")
            .MaximumLength(256)
                .WithMessage("Invalid refresh token format.");
    }
}