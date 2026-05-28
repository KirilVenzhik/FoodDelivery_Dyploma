using FluentValidation;
using RestaurantService.Application.DTOs;

namespace RestaurantService.Application.Validators;

public class CreateRestaurantRequestValidator
    : AbstractValidator<CreateRestaurantRequest>
{
    public CreateRestaurantRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}