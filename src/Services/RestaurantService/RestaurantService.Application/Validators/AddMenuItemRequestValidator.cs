using FluentValidation;
using RestaurantService.Application.DTOs;

namespace RestaurantService.Application.Validators;

public class AddMenuItemRequestValidator : AbstractValidator<AddMenuItemRequest>
{
    public AddMenuItemRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
