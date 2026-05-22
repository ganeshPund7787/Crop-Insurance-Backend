using Authentication.DTOs.Auth;
using Authentication.Models.Enums;
using FluentValidation;

namespace Authentication.Helpers.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        // ── Basic fields ───────────────────────────────────────────────────
        RuleFor(x => x.FullName)
            .NotEmpty()
                .WithMessage("Full name is required.")
            .MinimumLength(3)
                .WithMessage("Full name must be at least 3 characters.")
            .MaximumLength(100)
                .WithMessage("Full name must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s]+$")
                .WithMessage("Full name must contain only letters and spaces.");

        RuleFor(x => x.Email)
            .MustBeValidEmail();

        RuleFor(x => x.Password)
            .MustBeStrongPassword();

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
                .WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{9,14}$")
                .WithMessage("A valid phone number is required.");

        RuleFor(x => x.Role)
            .IsInEnum()
                .WithMessage("Invalid role specified.");

        // ── Farmer-specific rules ──────────────────────────────────────────
        When(x => x.Role == UserRole.Farmer, () =>
        {
            RuleFor(x => x.District)
                .NotEmpty()
                    .WithMessage("District is required for farmer registration.");

            RuleFor(x => x.State)
                .NotEmpty()
                    .WithMessage("State is required for farmer registration.");

            RuleFor(x => x.AadhaarNumber)
                .NotEmpty()
                    .WithMessage("Aadhaar number is required for farmer registration.")
                .Length(12)
                    .WithMessage("Aadhaar number must be exactly 12 digits.")
                .Matches(@"^\d{12}$")
                    .WithMessage("Aadhaar number must contain only digits.");

            RuleFor(x => x.TotalLandAcres)
                .GreaterThan(0)
                    .WithMessage("Total land acres must be greater than 0.")
                .When(x => x.TotalLandAcres.HasValue);
        });

        // ── Agent-specific rules ───────────────────────────────────────────
        When(x => x.Role == UserRole.InsuranceAgent, () =>
        {
            RuleFor(x => x.AgentCode)
                .NotEmpty()
                    .WithMessage("Agent code is required for agent registration.")
                .MaximumLength(20)
                    .WithMessage("Agent code must not exceed 20 characters.");

            RuleFor(x => x.LicenseNumber)
                .NotEmpty()
                    .WithMessage("License number is required for agent registration.")
                .MaximumLength(50)
                    .WithMessage("License number must not exceed 50 characters.");

            RuleFor(x => x.AssignedDistrict)
                .NotEmpty()
                    .WithMessage("Assigned district is required for agent registration.");
        });
    }
}