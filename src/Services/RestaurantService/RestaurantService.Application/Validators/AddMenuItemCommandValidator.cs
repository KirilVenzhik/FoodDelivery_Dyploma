using FluentValidation;
using RestaurantService.Application.Commands;

namespace RestaurantService.Application.Validators;

public sealed class AddMenuItemCommandValidator : AbstractValidator<AddMenuItemCommand>
{
    public AddMenuItemCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
