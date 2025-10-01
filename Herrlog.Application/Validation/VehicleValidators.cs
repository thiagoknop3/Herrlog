
using FluentValidation;
using Herrlog.Application.DTOs;

namespace Herrlog.Application.Validation;

public class VehicleCreateValidator : AbstractValidator<VehicleCreateDto>
{
    public VehicleCreateValidator()
    {
        RuleFor(x => x.Plate).NotEmpty()
            .WithMessage("Plate is required.")
            .Length(7);
        RuleFor(x => x.Model)
            .MaximumLength(100)
            .When(x => x.Model is not null);
    }
}

public class VehicleUpdateValidator : AbstractValidator<VehicleUpdateDto>
{
    public VehicleUpdateValidator()
    {
        RuleFor(x => x.Id).NotNull()
            .WithMessage("Id is required.");
        RuleFor(x => x.Plate).NotEmpty()
            .WithMessage("Plate is required.")
            .Length(7);
        RuleFor(x => x.Model)
            .MaximumLength(100)
            .When(x => x.Model is not null);
    }
}
