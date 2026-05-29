using FluentValidation;
using RestaurantService.Application.Commands;

namespace RestaurantService.Application.Validators;

public sealed class UpdateMenuItemCommandValidator : AbstractValidator<UpdateMenuItemCommand>
{
    public UpdateMenuItemCommandValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .When(x => x.Price.HasValue);
    }
}
