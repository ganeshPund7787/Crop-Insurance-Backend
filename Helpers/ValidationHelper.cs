using FluentValidation;

namespace Authentication.Helpers;

public static class ValidationHelper
{
    // ─── Reusable password rule ────────────────────────────────────────────
    // Applied consistently across Register and ChangePassword validators
    public static IRuleBuilderOptions<T, string> MustBeStrongPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
                .WithMessage("Password is required.")
            .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100)
                .WithMessage("Password must not exceed 100 characters.")
            .Matches("[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]")
                .WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]")
                .WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]")
                .WithMessage("Password must contain at least one special character.");
    }

    // ─── Reusable email rule ───────────────────────────────────────────────
    public static IRuleBuilderOptions<T, string> MustBeValidEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
                .WithMessage("Email is required.")
            .EmailAddress()
                .WithMessage("A valid email address is required.")
            .MaximumLength(150)
                .WithMessage("Email must not exceed 150 characters.");
    }
}