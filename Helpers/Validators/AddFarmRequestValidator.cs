using Authentication.DTOs.Farmer;
using FluentValidation;

namespace Authentication.Helpers.Validators;

public class AddFarmRequestValidator : AbstractValidator<AddFarmRequestDto>
{
    public AddFarmRequestValidator()
    {
        RuleFor(x => x.FarmName)
            .NotEmpty()
                .WithMessage("Farm name is required.")
            .MaximumLength(150)
                .WithMessage("Farm name must not exceed 150 characters.");

        RuleFor(x => x.AreaInAcres)
            .GreaterThan(0)
                .WithMessage("Area must be greater than 0.")
            .LessThanOrEqualTo(10000)
                .WithMessage("Area cannot exceed 10,000 acres.");

        RuleFor(x => x.SoilType)
            .NotEmpty()
                .WithMessage("Soil type is required.")
            .MaximumLength(50)
                .WithMessage("Soil type must not exceed 50 characters.");

        RuleFor(x => x.Location)
            .NotEmpty()
                .WithMessage("Location is required.")
            .MaximumLength(200)
                .WithMessage("Location must not exceed 200 characters.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
                .WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
                .WithMessage("Longitude must be between -180 and 180.");
    }
}