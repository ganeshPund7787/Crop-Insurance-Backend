using Authentication.DTOs.Auth;
using FluentValidation;

namespace Authentication.Helpers.Validators;

public class ChangePasswordRequestValidator
    : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
                .WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .MustBeStrongPassword();

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
                .WithMessage("Password confirmation is required.")
            .Equal(x => x.NewPassword)
                .WithMessage("Passwords do not match.");

        // New password must differ from current
        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword)
                .WithMessage(
                    "New password must be different from current password.");
    }
}