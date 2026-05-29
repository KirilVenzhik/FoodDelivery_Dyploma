using FluentValidation;
using RestaurantService.Application.DTOs;

namespace RestaurantService.Application.Validators;

public class CreateRestaurantRequestValidator : AbstractValidator<CreateRestaurantRequest>
{
    public CreateRestaurantRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(200);
    }
}
