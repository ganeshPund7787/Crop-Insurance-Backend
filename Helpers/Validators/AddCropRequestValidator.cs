using Authentication.DTOs.Farmer;
using FluentValidation;

namespace Authentication.Helpers.Validators;

public class AddCropRequestValidator : AbstractValidator<AddCropRequestDto>
{
    public AddCropRequestValidator()
    {
        RuleFor(x => x.CropName)
            .NotEmpty()
                .WithMessage("Crop name is required.")
            .MaximumLength(100)
                .WithMessage("Crop name must not exceed 100 characters.");

        RuleFor(x => x.Season)
            .NotEmpty()
                .WithMessage("Season is required.")
            .Must(s => new[] { "Kharif", "Rabi", "Zaid" }.Contains(s))
                .WithMessage("Season must be Kharif, Rabi, or Zaid.");

        RuleFor(x => x.ExpectedYieldTons)
            .GreaterThan(0)
                .WithMessage("Expected yield must be greater than 0.");

        RuleFor(x => x.SowingDate)
            .NotEmpty()
                .WithMessage("Sowing date is required.")
            .LessThan(DateTime.UtcNow.AddDays(1))
                .WithMessage("Sowing date cannot be in the future.");

        RuleFor(x => x.ExpectedHarvestDate)
            .NotEmpty()
                .WithMessage("Expected harvest date is required.")
            .GreaterThan(x => x.SowingDate)
                .WithMessage(
                    "Expected harvest date must be after sowing date.");
    }
}