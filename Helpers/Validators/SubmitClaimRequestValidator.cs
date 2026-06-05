using Authentication.DTOs.Farmer;
using FluentValidation;

namespace Authentication.Helpers.Validators;

public class SubmitClaimRequestValidator
    : AbstractValidator<SubmitClaimRequestDto>
{
    public SubmitClaimRequestValidator()
    {
        RuleFor(x => x.CropId)
            .NotEmpty()
                .WithMessage("Crop is required.");

        RuleFor(x => x.DamageType)
            .IsInEnum()
                .WithMessage("Invalid damage type.");

        RuleFor(x => x.DamageDescription)
            .NotEmpty()
                .WithMessage("Damage description is required.")
            .MinimumLength(20)
                .WithMessage(
                    "Please describe the damage in at least 20 characters.")
            .MaximumLength(1000)
                .WithMessage(
                    "Description must not exceed 1000 characters.");

        RuleFor(x => x.EstimatedLossAmount)
            .GreaterThan(0)
                .WithMessage("Estimated loss amount must be greater than 0.")
            .LessThanOrEqualTo(10_000_000)
                .WithMessage(
                    "Estimated loss amount cannot exceed 1 crore.");

        RuleFor(x => x.IncidentDate)
            .NotEmpty()
                .WithMessage("Incident date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Incident date cannot be in the future.")
            .GreaterThan(DateTime.UtcNow.AddYears(-1))
                .WithMessage(
                    "Incident date cannot be more than 1 year ago.");
    }
}