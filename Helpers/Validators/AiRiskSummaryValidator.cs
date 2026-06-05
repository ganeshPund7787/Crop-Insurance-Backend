using Backend_Crop_Insurrance.DTOs.Farmer;
using FluentValidation;
namespace Backend_Crop_Insurrance.Helpers.Validators
{


    public class AiRiskSummaryValidator : AbstractValidator<AiRiskSummaryRequestDto>
    {
        private static readonly string[] ValidSeasons = ["Kharif", "Rabi", "Zaid", "Annual"];

        public AiRiskSummaryValidator()
        {
            RuleFor(x => x.CropName)
                .NotEmpty().WithMessage("Crop name is required.")
                .MaximumLength(100).WithMessage("Crop name must not exceed 100 characters.");

            RuleFor(x => x.District)
                .NotEmpty().WithMessage("District is required.")
                .MaximumLength(100).WithMessage("District must not exceed 100 characters.");

            RuleFor(x => x.Season)
                .NotEmpty().WithMessage("Season is required.")
                .Must(s => ValidSeasons.Contains(s))
                .WithMessage("Season must be one of: Kharif, Rabi, Zaid, Annual.");

            RuleFor(x => x.LandArea)
                .GreaterThan(0).WithMessage("Land area must be greater than 0.")
                .LessThanOrEqualTo(10000).WithMessage("Land area seems unrealistically large.");

            RuleFor(x => x.ProblemDescription)
                .MaximumLength(1000).WithMessage("Problem description must not exceed 1000 characters.")
                .When(x => x.ProblemDescription != null);
        }
    }
}
